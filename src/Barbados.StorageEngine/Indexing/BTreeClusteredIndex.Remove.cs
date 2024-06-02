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
			bool _tryRemoveFromLeaf(ObjectPage from, BTreeIndexTraceback traceback, bool removeChunk)
			{
				if (!(removeChunk ? from.TryRemoveObjectChunk(id, out _) : from.TryRemoveObject(id)))
				{
					Pool.Release(from);
					return false;
				}

				// Remove the leaf if it's empty, unless it is the collection page
				if (from.Count() == 0)
				{
					if (from.Header.Handle.Handle != collectionPageHandle.Handle)
					{
						ChainHelpers.RemoveAndDeallocate(from, Pool);
					}

					else
					{
						Pool.SaveRelease(from);
					}

					var r = traceback.TryMoveUp();
					Debug.Assert(r);

					Span<byte> idbuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
					id.WriteTo(idbuf);

					RemoveSeparatorPropagate(NormalisedValueSpan.FromNormalised(idbuf), traceback);
				}

				else
				{
					var r = from.TryReadHighestId(out var hid);
					Debug.Assert(r);

					Pool.SaveRelease(from);

					// Update the parent if the removed id was the highest
					var hidn = new ObjectIdNormalised(hid);
					if (hidn.CompareTo(id) < 0)
					{
						Span<byte> idbuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
						Span<byte> hidbuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
						id.WriteTo(idbuf);
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
						from.Next.Handle != collectionPageHandle.Handle &&
						from.Previous.Handle != collectionPageHandle.Handle &&
						from.Header.Handle.Handle != collectionPageHandle.Handle
					)
					{
						BalanceLeaf(traceback);
					}			
				}

				return true;
			}

			Span<byte> idbuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(idbuf);

			var ikey = _toBTreeIndexKey(idbuf);
			if (TryFind(ikey, out var traceback))
			{
				var leaf = Pool.LoadPin<ObjectPage>(traceback.Current);

				// An object that didn't fit on the leaf page is spread across several overflow pages.
				// Since a single overflow chain is occupied by a single object, we can just
				// deallocate all overflow pages and remove the head entry from the leaf
				if (leaf.TryReadObjectChunk(id, out _, out _, out var next))
				{
					while (!next.IsNull)
					{
						var opage = Pool.LoadPin<ObjectPageOverflow>(next);
						next = opage.Next;

						Pool.Release(opage);
						Pool.Deallocate(opage.Header.Handle);
					}

					var r = _tryRemoveFromLeaf(leaf, traceback, removeChunk: true);
					Debug.Assert(r);
					return true;
				}

				return _tryRemoveFromLeaf(leaf, traceback, removeChunk: false);
			}

			return false;
		}
	}
}
