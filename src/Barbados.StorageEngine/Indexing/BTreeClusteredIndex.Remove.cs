using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeClusteredIndex
	{
		public bool TryRemove(ObjectIdNormalised id, PageHandle collectionPageHandle)
		{
			Span<byte> idbuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(idbuf);

			var ikey = _toBTreeIndexKey(idbuf);
			if (!TryFind(ikey, out var traceback))
			{
				return false;
			}

			var leaf = Pool.LoadPin<ObjectPage>(traceback.Current);

			// An object that didn't fit on the leaf page is spread across several overflow pages.
			// Since a single overflow chain is occupied by a single object, we can just
			// deallocate all overflow pages and remove the head entry from the leaf
			if (leaf.TryRemoveObjectChunk(id, out var next))
			{
				while (!next.IsNull)
				{
					var opage = Pool.LoadPin<ObjectPageOverflow>(next);
					next = opage.Next;

					Pool.Release(opage);
					Pool.Deallocate(opage.Header.Handle);
				}
			}

			else
			if (!leaf.TryRemoveObject(id))
			{
				Pool.Release(leaf);
				return false;
			}

			// Remove the leaf if it's empty, unless it is the collection page
			if (leaf.Count() == 0)
			{
				if (leaf.Header.Handle.Handle != collectionPageHandle.Handle)
				{
					ChainHelpers.RemoveAndDeallocate(leaf, Pool);
				}

				else
				{
					Pool.SaveRelease(leaf);
				}

				var tracebackCopy = traceback.Clone();
				var r = tracebackCopy.TryMoveUp();
				Debug.Assert(r);

				RemoveSeparatorPropagate(NormalisedValueSpan.FromNormalised(idbuf), tracebackCopy);
			}

			else
			{
				var r = leaf.TryReadHighestId(out var hid);
				Debug.Assert(r);

				// Update the parent if the removed id was the highest
				var hidn = new ObjectIdNormalised(hid);
				if (hidn.CompareTo(id) < 0)
				{
					Span<byte> hidbuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
					hidn.WriteTo(hidbuf);

					var tracebackCopy = traceback.Clone();
					r = tracebackCopy.TryMoveUp();
					Debug.Assert(r);

					UpdateSeparatorPropagate(
						NormalisedValueSpan.FromNormalised(idbuf),
						NormalisedValueSpan.FromNormalised(hidbuf),
						tracebackCopy
					);
				}

				// 'CollectionPage' might be deallocated in the process
				if (
					leaf.Next.Handle != collectionPageHandle.Handle &&
					leaf.Previous.Handle != collectionPageHandle.Handle &&
					leaf.Header.Handle.Handle != collectionPageHandle.Handle
				)
				{
					Pool.SaveRelease(leaf);
					BalanceLeaf(traceback);
				}

				else
				{
					Pool.SaveRelease(leaf);
				}
			}

			return true;
		}
	}
}
