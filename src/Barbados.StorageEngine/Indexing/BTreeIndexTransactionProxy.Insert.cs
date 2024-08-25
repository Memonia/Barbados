using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeIndexTransactionProxy
	{
		public void Insert(BTreeIndexKey key, ObjectId id)
		{
			if (TryFindWithPreemptiveSplit(key, out var traceback))
			{
				var leaf = Transaction.Load<BTreeLeafPage>(traceback.Current);

				// An overflow entry exists. The locators are in the overflow chain
				if (leaf.TryReadOverflowHandle(key.Separator, out var start))
				{
					var last = start;
					foreach (var overflow in
						ChainHelpers.EnumerateForwardsPinned<BTreeLeafPageOverflow>(Transaction, start)
					)
					{
						last = overflow.Header.Handle;
						if (overflow.TryWriteObjectId(new(id), key.IsTrimmed))
						{
							Transaction.Save(overflow);
							return;
						}
					}

					// The overflow chain is full. Append a page to the chain and insert
					var next = Transaction.AllocateHandle();
					var nextPage = new BTreeLeafPageOverflow(next);
					var lastPage = Transaction.Load<BTreeLeafPageOverflow>(last);

					ChainHelpers.AppendOneWay(nextPage, lastPage);

					var r = nextPage.TryWriteObjectId(new(id), key.IsTrimmed);
					Debug.Assert(r);

					Transaction.Save(nextPage);
					Transaction.Save(lastPage);
				}

				// An overflow entry doesn't exist, but they key has been inserted.
				// An overflow chain needs to be created and the existing key moved there
				else
				if (leaf.TryReadObjectId(key.Separator, out var storedId, out var isTrimmed))
				{
					var oh = Transaction.AllocateHandle();
					var r = leaf.TryOverwriteWithOverflow(key.Separator, oh);
					Debug.Assert(r);

					var overflow = new BTreeLeafPageOverflow(oh);
					r = overflow.TryWriteObjectId(new(storedId), isTrimmed);
					Debug.Assert(r);

					r = overflow.TryWriteObjectId(new(id), key.IsTrimmed);
					Debug.Assert(r);

					Transaction.Save(overflow);
					Transaction.Save(leaf);
				}

				// No entry is associated with a given key
				else
				{
					// Empty pages are deallocated
					var r = leaf.TryReadHighest(out var hlkey);
					Debug.Assert(r);

					Span<byte> hlkeySepCopy = stackalloc byte[hlkey.Bytes.Length];
					hlkey.Bytes.CopyTo(hlkeySepCopy);

					if (leaf.TryWriteObjectId(key, id))
					{
						// If inserted key is the new maximum, update the parent separators
						if (key.Separator.Bytes.SequenceCompareTo(hlkeySepCopy) > 0)
						{
							r = traceback.TryMoveUp();
							Debug.Assert(r);

							UpdateSeparatorPropagate(
								NormalisedValueSpan.FromNormalised(hlkeySepCopy), key.Separator,
								traceback
							);
						}

						Transaction.Save(leaf);
					}

					else
					{
						_split(key, id, traceback);
					}
				}
			}

			// Should only happen on the first insert in an empty tree
			else
			{
				var root = Transaction.Load<BTreeRootPage>(Info.RootHandle);
				Debug.Assert(root.Count() == 0);

				var lh = Transaction.AllocateHandle();
				var leaf = new BTreeLeafPage(lh);

				var a = leaf.TryWriteObjectId(key, id);
				var b = root.TryWriteSeparatorHandle(key.Separator, lh);

				Debug.Assert(a);
				Debug.Assert(b);

				Transaction.Save(leaf);
				Transaction.Save(root);
			}
		}

		private void _split(BTreeIndexKey key, ObjectId id, BTreeIndexTraceback traceback)
		{
			var target = Transaction.Load<BTreeLeafPage>(traceback.Current);
			var lh = Transaction.AllocateHandle();
			var left = new BTreeLeafPage(lh);

			HandleBeforeSplit(target, left);
			var insertTarget = SplitLeaf(target, left, key.Separator, traceback);
			var r = insertTarget.TryWriteObjectId(key, id);
			Debug.Assert(r);

			Transaction.Save(left);
			Transaction.Save(target);
		}
	}
}
