using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeIndex
	{
		public void Insert(NormalisedValue key, ObjectId id)
		{
			var ikey = ToBTreeIndexKey(key);
			if (TryFindWithPreemptiveSplit(ikey, out var traceback))
			{
				var leaf = Pool.LoadPin<BTreeLeafPage>(traceback.Current);

				// An overflow entry exists. The locators are in the overflow chain
				if (leaf.TryReadOverflowHandle(ikey.Separator, out var start))
				{
					Pool.Release(leaf);

					var last = start;
					foreach (var overflow in
						ChainHelpers.EnumerateForwardsPinned<BTreeLeafPageOverflow>(Pool, start, release: false)
					)
					{
						last = overflow.Header.Handle;
						if (overflow.TryWriteObjectId(new(id), ikey.IsTrimmed))
						{
							Pool.SaveRelease(overflow);
							return;
						}

						Pool.Release(overflow);
					}

					// The overflow chain is full. Append a page to the chain and insert
					var next = Pool.Allocate();
					var nextPage = new BTreeLeafPageOverflow(next);
					var lastPage = Pool.LoadPin<BTreeLeafPageOverflow>(last);

					ChainHelpers.AppendOneWay(nextPage, lastPage);

					var r = nextPage.TryWriteObjectId(new(id), ikey.IsTrimmed);
					Debug.Assert(r);

					Pool.SaveRelease(lastPage);
					Pool.SaveRelease(nextPage);
				}

				// An overflow entry doesn't exist, but they key has been inserted.
				// An overflow chain needs to be created and the existing key moved there
				else
				if (leaf.TryReadObjectId(ikey.Separator, out var storedId, out var isTrimmed))
				{
					var oh = Pool.Allocate();
					var r = leaf.TryOverwriteWithOverflow(ikey, oh);
					Debug.Assert(r);

					var overflow = new BTreeLeafPageOverflow(oh);
					r = overflow.TryWriteObjectId(new(storedId), isTrimmed);
					Debug.Assert(r);

					r = overflow.TryWriteObjectId(new(id), ikey.IsTrimmed);
					Debug.Assert(r);

					Pool.SaveRelease(leaf);
					Pool.SaveRelease(overflow);
				}

				// No entry is associated with a given key
				else
				{
					// Empty pages are deallocated
					var r = leaf.TryReadHighest(out var hlkey);
					Debug.Assert(r);

					Span<byte> hlkeySepCopy = stackalloc byte[hlkey.Separator.Bytes.Length];
					hlkey.Separator.Bytes.CopyTo(hlkeySepCopy);

					if (leaf.TryWriteObjectId(ikey, id))
					{
						// If inserted key is the new maximum, update the parent separators
						if (ikey.Separator.Bytes.SequenceCompareTo(hlkeySepCopy) > 0)
						{
							r = traceback.TryMoveUp();
							Debug.Assert(r);

							UpdateSeparatorPropagate(
								NormalisedValueSpan.FromNormalised(hlkeySepCopy), ikey.Separator,
								traceback
							);
						}

						Pool.SaveRelease(leaf);
					}

					else
					{
						Pool.Release(leaf);
						_split(ikey, id, traceback);
					}
				}
			}

			// Should only happen on the first insert in an empty tree
			else
			{
				var root = Pool.LoadPin<BTreeRootPage>(Info.RootPageHandle);
				Debug.Assert(root.Count() == 0);

				var lh = Pool.Allocate();
				var leaf = new BTreeLeafPage(lh);

				var a = leaf.TryWriteObjectId(ikey, id);
				var b = root.TryWriteSeparatorHandle(ikey.Separator, lh);

				Debug.Assert(a);
				Debug.Assert(b);

				Pool.SaveRelease(root);
				Pool.SaveRelease(leaf);
			}
		}

		private void _split(BTreeIndexKey key, ObjectId id, BTreeIndexTraceback traceback)
		{
			var target = Pool.LoadPin<BTreeLeafPage>(traceback.Current);
			var lh = Pool.Allocate();
			var left = new BTreeLeafPage(lh);

			if (target.Previous.IsNull)
			{
				ChainHelpers.Prepend(left, target);
			}

			else
			{
				var previous = Pool.LoadPin<BTreeLeafPage>(target.Previous);
				ChainHelpers.Insert(left, previous, target);
				Pool.SaveRelease(previous);
			}

			target.Spill(left, fromHighest: false);

			var r = traceback.TryMoveUp();
			Debug.Assert(r);

			r = target.TryReadLowest(out var rlkey);
			Debug.Assert(r);
			Debug.Assert(!rlkey.Separator.Bytes.SequenceEqual(key.Separator.Bytes));

			if (key.Separator.Bytes.SequenceCompareTo(rlkey.Separator.Bytes) < 0)
			{
				// No update for the parents required, because we didn't insert the separator yet
				r = left.TryWriteObjectId(key, id);
				Debug.Assert(r);
			}

			else
			{
				r = target.TryReadHighest(out var rhkey);
				Debug.Assert(r);
				Debug.Assert(!rhkey.Separator.Bytes.SequenceEqual(key.Separator.Bytes));

				if (key.Separator.Bytes.SequenceCompareTo(rhkey.Separator.Bytes) > 0)
				{
					UpdateSeparatorPropagate(rhkey.Separator, key.Separator, traceback.Clone());
				}

				r = target.TryWriteObjectId(key, id);
				Debug.Assert(r);
			}

			r = left.TryReadHighest(out var lhkey);
			Debug.Assert(r);

			Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Separator.Bytes.Length];
			lhkey.Separator.Bytes.CopyTo(lhkeySepCopy);

			Pool.SaveRelease(left);
			Pool.SaveRelease(target);

			InsertSeparator(
				NormalisedValueSpan.FromNormalised(lhkeySepCopy), lh,
				traceback
			);
		}
	}
}
