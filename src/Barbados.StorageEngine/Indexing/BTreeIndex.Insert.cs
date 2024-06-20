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

					Pool.Save(nextPage);
					Pool.SaveRelease(lastPage);
				}

				// An overflow entry doesn't exist, but they key has been inserted.
				// An overflow chain needs to be created and the existing key moved there
				else
				if (leaf.TryReadObjectId(ikey.Separator, out var storedId, out var isTrimmed))
				{
					var oh = Pool.Allocate();
					var r = leaf.TryOverwriteWithOverflow(ikey.Separator, oh);
					Debug.Assert(r);

					var overflow = new BTreeLeafPageOverflow(oh);
					r = overflow.TryWriteObjectId(new(storedId), isTrimmed);
					Debug.Assert(r);

					r = overflow.TryWriteObjectId(new(id), ikey.IsTrimmed);
					Debug.Assert(r);

					Pool.Save(overflow);
					Pool.SaveRelease(leaf);
				}

				// No entry is associated with a given key
				else
				{
					// Empty pages are deallocated
					var r = leaf.TryReadHighest(out var hlkey);
					Debug.Assert(r);

					Span<byte> hlkeySepCopy = stackalloc byte[hlkey.Bytes.Length];
					hlkey.Bytes.CopyTo(hlkeySepCopy);

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

				Pool.Save(leaf);
				Pool.SaveRelease(root);
			}
		}

		private void _split(BTreeIndexKey key, ObjectId id, BTreeIndexTraceback traceback)
		{
			var target = Pool.LoadPin<BTreeLeafPage>(traceback.Current);
			var lh = Pool.Allocate();
			var left = new BTreeLeafPage(lh);

			HandleBeforeSplit(target, left);
			var insertTarget = SplitLeaf(target, left, key.Separator, traceback);
			var r = insertTarget.TryWriteObjectId(key, id);
			Debug.Assert(r);

			Pool.Save(left);
			Pool.SaveRelease(target);
		}
	}
}
