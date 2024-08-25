using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeClusteredIndexTransactionProxy
	{
		public PageHandle Insert(ObjectIdNormalised id, ObjectBuffer buffer)
		{
			var ikey = _toBTreeIndexKey(id);
			if (TryFindWithPreemptiveSplit(ikey, out var traceback))
			{
				var leaf = Transaction.Load<ObjectPage>(traceback.Current);
				var r = leaf.TryReadHighestId(out var hid);
				Debug.Assert(r);

				var updateParent = false;
				PageHandle objectLocation;

				// Avoid splitting objects unnecessarily
				if (buffer.Length <= Constants.ObjectPageMaxChunkLength)
				{
					if (leaf.TryWriteObject(id, buffer.AsReadonlySpan()))
					{
						Transaction.Save(leaf);
						updateParent = true;
						objectLocation = traceback.Current;
					}

					// Instead of writing the object in chunks, split the page and write the object as a whole
					else
					{
						objectLocation = _split(id, buffer, traceback);
					}
				}

				else
				{
					if (_tryInsertChunk(leaf, id, buffer))
					{
						Transaction.Save(leaf);
						updateParent = true;
						objectLocation = traceback.Current;
					}

					else
					{
						objectLocation = _split(id, buffer, traceback);
					}
				}

				// If inserted id is the new maximum, update the parent separators
				var nhid = new ObjectIdNormalised(hid);
				if (updateParent && nhid.CompareTo(id) < 0)
				{
					r = traceback.TryMoveUp();
					Debug.Assert(r);

					UpdateSeparatorPropagate(
						NormalisedValueSpan.FromNormalised(nhid),
						NormalisedValueSpan.FromNormalised(id),
						traceback
					);
				}

				return objectLocation;
			}

			else
			{
				var root = Transaction.Load<CollectionPage>(Info.RootHandle);
				Debug.Assert(root.Count() == 0);

				var lh = Transaction.AllocateHandle();
				var leaf = new ObjectPage(lh);

				var a = root.TryWriteSeparatorHandle(ikey.Separator, leaf.Header.Handle);
				var b = _tryInsert(leaf, id, buffer);
				Debug.Assert(a);
				Debug.Assert(b);

				var objectLocation = leaf.Header.Handle;

				Transaction.Save(leaf);
				Transaction.Save(root);
				return objectLocation;
			}
		}

		private PageHandle _split(ObjectIdNormalised id, ObjectBuffer buffer, BTreeIndexTraceback traceback)
		{
			var target = Transaction.Load<ObjectPage>(traceback.Current);
			var lh = Transaction.AllocateHandle();
			var left = new ObjectPage(lh);

			HandleBeforeSplit(target, left);
			var insertTarget = SplitLeaf(target, left, NormalisedValueSpan.FromNormalised(id), traceback);
			var r = _tryInsert(insertTarget, id, buffer);
			Debug.Assert(r);

			Transaction.Save(left);
			Transaction.Save(target);
			return insertTarget.Header.Handle;
		}

		private bool _tryInsert(ObjectPage leaf, ObjectIdNormalised id, ObjectBuffer buffer)
		{
			if (leaf.TryWriteObject(id, buffer.AsReadonlySpan()))
			{
				return true;
			}

			return _tryInsertChunk(leaf, id, buffer);
		}

		private bool _tryInsertChunk(ObjectPage leaf, ObjectIdNormalised id, ObjectBuffer buffer)
		{
			var span = buffer.AsReadonlySpan();
			if (leaf.TryWriteObjectChunk(id, span, out var totalWritten))
			{
				var oh = Transaction.AllocateHandle();
				var overflow = new ObjectPageOverflow(oh);
				Transaction.Save(overflow);

				var r = leaf.TrySetOverfowHandle(id, oh);
				Debug.Assert(r);

				var next = oh;
				while (totalWritten < span.Length)
				{
					overflow = Transaction.Load<ObjectPageOverflow>(next);
					r = overflow.TryWriteObjectChunk(id, span[totalWritten..], out var written);
					Debug.Assert(r);

					totalWritten += written;
					Debug.Assert(totalWritten <= span.Length);

					if (overflow.Next.IsNull && totalWritten < span.Length)
					{
						var noh = Transaction.AllocateHandle();
						var nopage = new ObjectPageOverflow(noh);
						Transaction.Save(nopage);
						overflow.Next = noh;
					}

					next = overflow.Next;
					Transaction.Save(overflow);
				}

				Debug.Assert(totalWritten == span.Length);
				return true;
			}

			return false;
		}
	}
}
