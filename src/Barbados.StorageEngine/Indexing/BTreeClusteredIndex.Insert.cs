using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeClusteredIndex
	{
		public PageHandle Insert(ObjectIdNormalised id, ObjectBuffer obj)
		{
			Span<byte> kBuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(kBuf);

			var ikey = _toBTreeIndexKey(kBuf);
			if (TryFindWithPreemptiveSplit(ikey, out var traceback))
			{
				var leaf = Pool.LoadPin<ObjectPage>(traceback.Current);
				var r = leaf.TryReadHighestId(out var hid);
				Debug.Assert(r);

				var updateParent = false;
				PageHandle objectLocation;

				// Avoid splitting objects unnecessarily
				if (obj.Length <= Constants.ObjectPageMaxChunkLength)
				{
					if (leaf.TryWriteObject(id, obj.AsReadonlySpan()))
					{
						Pool.SaveRelease(leaf);
						updateParent = true;
						objectLocation = traceback.Current;
					}

					// Instead of writing the object in chunks, split the page and write the object as a whole
					else
					{
						Pool.Release(leaf);
						objectLocation = _split(id, obj, traceback);
					}
				}

				else
				{
					if (_tryInsertChunk(leaf, id, obj))
					{
						Pool.SaveRelease(leaf);
						updateParent = true;
						objectLocation = traceback.Current;
					}

					else
					{
						Pool.Release(leaf);
						objectLocation = _split(id, obj, traceback);
					}
				}

				// If inserted id is the new maximum, update the parent separators
				var nhid = new ObjectIdNormalised(hid);
				if (updateParent && nhid.CompareTo(id) < 0)
				{
					Span<byte> idBuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
					Span<byte> hidBuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
					id.WriteTo(idBuf);
					nhid.WriteTo(hidBuf);

					r = traceback.TryMoveUp();
					Debug.Assert(r);

					UpdateSeparatorPropagate(
						NormalisedValueSpan.FromNormalised(hidBuf),
						NormalisedValueSpan.FromNormalised(idBuf),
						traceback
					);
				}

				return objectLocation;
			}

			else
			{
				var root = Pool.LoadPin<CollectionPage>(Info.RootPageHandle);
				Debug.Assert(root.Count() == 0);

				var lh = Pool.Allocate();
				var leaf = new ObjectPage(lh);

				var a = root.TryWriteSeparatorHandle(ikey.Separator, leaf.Header.Handle);
				var b = _tryInsert(leaf, id, obj);
				Debug.Assert(a);
				Debug.Assert(b);

				var objectLocation = leaf.Header.Handle;

				Pool.SaveRelease(leaf);
				Pool.SaveRelease(root);

				return objectLocation;
			}
		}

		private PageHandle _split(ObjectIdNormalised id, ObjectBuffer obj, BTreeIndexTraceback traceback)
		{
			var target = Pool.LoadPin<ObjectPage>(traceback.Current);
			var lh = Pool.Allocate();
			var left = new ObjectPage(lh);

			Span<byte> idBuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(idBuf);

			var insertTarget = SplitLeaf(target, left, NormalisedValueSpan.FromNormalised(idBuf), traceback);
			var r = _tryInsert(insertTarget, id, obj);
			Debug.Assert(r);

			Pool.Save(left);
			Pool.SaveRelease(target);

			return insertTarget.Header.Handle;
		}

		private bool _tryInsert(ObjectPage leaf, ObjectIdNormalised id, ObjectBuffer obj)
		{
			if (leaf.TryWriteObject(id, obj.AsReadonlySpan()))
			{
				return true;
			}

			return _tryInsertChunk(leaf, id, obj);
		}

		private bool _tryInsertChunk(ObjectPage leaf, ObjectIdNormalised id, ObjectBuffer obj)
		{
			var span = obj.AsReadonlySpan();
			if (leaf.TryWriteObjectChunk(id, span, out var totalWritten))
			{
				var oh = Pool.Allocate();
				var overflow = new ObjectPageOverflow(oh);
				Pool.Save(overflow);

				var r = leaf.TrySetOverfowHandle(id, oh);
				Debug.Assert(r);

				var next = oh;
				while (totalWritten < span.Length)
				{
					overflow = Pool.LoadPin<ObjectPageOverflow>(next);
					r = overflow.TryWriteObjectChunk(id, span[totalWritten..], out var written);
					Debug.Assert(r);

					totalWritten += written;
					Debug.Assert(totalWritten <= span.Length);

					if (overflow.Next.IsNull && totalWritten < span.Length)
					{
						var noh = Pool.Allocate();
						var nopage = new ObjectPageOverflow(noh);
						Pool.Save(nopage);
						overflow.Next = noh;
					}

					next = overflow.Next;
					Pool.SaveRelease(overflow);
				}

				Debug.Assert(totalWritten == span.Length);
				return true;
			}

			return false;
		}
	}
}
