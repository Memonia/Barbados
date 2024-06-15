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
			var ikey = ToBTreeIndexKey(key);
			if (!TryFind(ikey, out var traceback))
			{
				return false;
			}

			var leaf = Pool.LoadPin<BTreeLeafPage>(traceback.Current);
			var removeOverflow = false;

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
							// The only page in the chain
							if (prev.IsNull && opage.Next.IsNull)
							{
								removeOverflow = true;
							}

							// There are more pages in the chain
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
							Pool.SaveRelease(opage);
						}

						break;
					}

					else
					{
						prev = next;
						next = opage.Next;
						Pool.Release(opage);
					}
				}

				Pool.Save(leaf);
			}

			else
			if (!leaf.TryRemoveObjectId(ikey))
			{
				Pool.Release(leaf);
				return false;
			}

			if (removeOverflow)
			{
				var r = leaf.TryRemoveOverflowHandle(ikey.Separator, out _);
				Debug.Assert(r);
			}

			// Remove the leaf if it's empty
			if (leaf.Count() == 0)
			{
				ChainHelpers.RemoveAndDeallocate(leaf, Pool);

				var tracebackCopy = traceback.Clone();
				var r = tracebackCopy.TryMoveUp();
				Debug.Assert(r);

				RemoveSeparatorPropagate(ikey.Separator, tracebackCopy);
			}

			else
			{
				var r = leaf.TryReadHighest(out var hkey);
				Debug.Assert(r);

				Span<byte> hkeySepCopy = stackalloc byte[hkey.Bytes.Length];
				hkey.Bytes.CopyTo(hkeySepCopy);

				Pool.SaveRelease(leaf);

				// Update the parent if the removed key was the highest
				if (hkeySepCopy.SequenceCompareTo(ikey.Separator.Bytes) <= 0)
				{
					var tracebackCopy = traceback.Clone();
					r = tracebackCopy.TryMoveUp();
					Debug.Assert(r);

					UpdateSeparatorPropagate(
						ikey.Separator, NormalisedValueSpan.FromNormalised(hkeySepCopy),
						tracebackCopy
					);
				}

				BalanceLeaf(traceback);
			}

			return true;
		}
	}
}
