using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeIndex
	{
		public bool TryRemove(NormalisedValue key, ObjectId id)
		{
			bool _tryRemoveFromLeaf(BTreeLeafPage from, BTreeIndexKey key, BTreeIndexTraceback traceback, bool removeOverflow)
			{
				if (!(removeOverflow ? from.TryRemoveOverflowHandle(key) : from.TryRemoveObjectId(key)))
				{
					Pool.Release(from);
					return false;
				}

				// Remove the leaf if it's empty
				if (from.Count() == 0)
				{
					ChainHelpers.RemoveAndDeallocate(from, Pool);

					var r = traceback.TryMoveUp();
					Debug.Assert(r);

					RemoveSeparatorPropagate(key.Separator, traceback);
				}

				else
				{
					var r = from.TryReadHighest(out var hkey);
					Debug.Assert(r);

					Span<byte> hkeySepCopy = stackalloc byte[hkey.Separator.Bytes.Length];
					hkey.Separator.Bytes.CopyTo(hkeySepCopy);

					Pool.SaveRelease(from);

					// Update the parent if the removed key was the highest
					if (hkeySepCopy.SequenceCompareTo(key.Separator.Bytes) <= 0)
					{
						var tracebackCopy = traceback.Clone();
						r = tracebackCopy.TryMoveUp();
						Debug.Assert(r);

						UpdateSeparatorPropagate(
							key.Separator, NormalisedValueSpan.FromNormalised(hkeySepCopy),
							tracebackCopy
						);
					}

					BalanceLeaf(traceback);
				}

				return true;
			}

			using var _ = _lock.Acquire(LockMode.Write);

			var ikey = ToBTreeIndexKey(key);
			if (TryFind(ikey, out var traceback))
			{
				var leaf = Pool.LoadPin<BTreeLeafPage>(traceback.Current);

				// The entry is in the overflow page. Remove it and deallocate the page if it's empty
				if (leaf.TryReadOverflowHandle(ikey.Separator, out var next))
				{
					Debug.Assert(!next.IsNull);
					var prev = PageHandle.Null;
					while (!next.IsNull)
					{
						var opage = Pool.LoadPin<BTreeLeafPageOverflow>(next);
						if (opage.TryRemoveObjectId(new(id)))
						{
							// Deallocate empty overflow page
							if (opage.Count() == 0)
							{
								// The only page in the chain. Remove from the leaf and deallocate
								if (prev.IsNull && opage.Next.IsNull)
								{
									var r = _tryRemoveFromLeaf(leaf, ikey, traceback, removeOverflow: true);
									Debug.Assert(r);
								}

								// There are more pages in the chain. Remove from the chain and deallocate
								else
								if (!prev.IsNull)
								{
									var popage = Pool.LoadPin<BTreeLeafPageOverflow>(prev);
									ChainHelpers.RemoveOneWay(opage, popage);

									Pool.Release(popage);
									Pool.SaveRelease(popage);
								}

								Pool.Release(opage);
								Pool.Deallocate(opage.Header.Handle);
							}

							else
							{
								Pool.Release(leaf);
								Pool.SaveRelease(opage);
							}

							return true;
						}

						else
						{
							prev = next;
							next = opage.Next;
							Pool.Release(opage);
						}
					}

					// The id was not found in the overflow chain
					Debug.Assert(false);
				}

				else
				{
					return _tryRemoveFromLeaf(leaf, ikey, traceback, removeOverflow: false);
				}
			}

			return false;
		}
	}
}
