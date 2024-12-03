using System;
using System.Diagnostics;

using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class AbstractBTreeIndexTransactionProxy<T>
	{
		private static void _splitRoot(BTreePage root, BTreePage left, BTreePage right)
		{
			root.Spill(right, fromHighest: true);
			root.Flush(left, fromHighest: true);
			Debug.Assert(root.Count() == 0);

			var a = left.TryReadHighestSeparatorHandle(out var lhsep, out _);
			var b = right.TryReadHighestSeparatorHandle(out var rhsep, out _);
			Debug.Assert(a);
			Debug.Assert(b);

			a = root.TryWriteSeparatorHandle(lhsep, left.Header.Handle);
			b = root.TryWriteSeparatorHandle(rhsep, right.Header.Handle);
			Debug.Assert(a);
			Debug.Assert(b);
		}

		private static void _splitNode(BTreePage parent, BTreePage node, BTreePage split)
		{
			node.Spill(split, fromHighest: false);

			var r = split.TryReadHighestSeparatorHandle(out var nhsep, out _);
			Debug.Assert(r);

			// Previous parents have been preemptively split, so the write must succeed.
			// We don't need to update the parent's parents, because even if the node's highest key
			// was the highest separator of the parent, we transfered the lowest half of the keys from the node
			r = parent.TryWriteSeparatorHandle(nhsep, split.Header.Handle);
			Debug.Assert(r);
		}

		protected void InsertSeparator(NormalisedValueSpan separator, PageHandle lessOrEqual, BTreeIndexTraceback traceback)
		{
			var target = Transaction.Load<BTreePage>(traceback.Current);
			if (
				target.TryReadHighestSeparatorHandle(out var highest, out _) &&
				separator.Bytes.SequenceCompareTo(separator.Bytes) > 0
			)
			{
				var tracebackCopy = traceback.Clone();
				if (tracebackCopy.TryMoveUp())
				{
					UpdateSeparatorPropagate(highest, separator, traceback);
				}
			}

			// There must be enough space after preemptive splitting
			var r = target.TryWriteSeparatorHandle(separator, lessOrEqual);
			Debug.Assert(r);

			Transaction.Save(target);
		}

		protected void UpdateSeparatorPropagate(NormalisedValueSpan separator, NormalisedValueSpan replacement, BTreeIndexTraceback traceback)
		{
			var target = Transaction.Load<BTreePage>(traceback.Current);
			if (!target.TryReadSeparatorHandle(separator, out var lessOrEqual))
			{
				return;
			}

			if (traceback.TryPeekUp(out _))
			{
				var tracebackCopy = traceback.Clone();
				var rr = tracebackCopy.TryMoveUp();
				Debug.Assert(rr);
				UpdateSeparatorPropagate(separator, replacement, tracebackCopy);
			}

			var r = target.TryRemoveSeparatorHandle(separator);
			Debug.Assert(r);

			Transaction.Save(target);
			InsertSeparator(replacement, lessOrEqual, traceback);
		}

		protected void RemoveSeparatorPropagate(NormalisedValueSpan separator, BTreeIndexTraceback traceback)
		{
			var target = Transaction.Load<BTreePage>(traceback.Current);
			if (!target.TryRemoveSeparatorHandle(separator))
			{
				return;
			}

			// Remove an empty page from the tree, unless it is the root
			if (target.Count() == 0 && traceback.CanMoveUp)
			{
				Transaction.Deallocate(target.Header.Handle);
				var r = traceback.TryMoveUp();
				Debug.Assert(r);
				RemoveSeparatorPropagate(separator, traceback);
			}

			else
			if (traceback.CanMoveUp)
			{
				var r = target.TryReadHighestSeparatorHandle(out var hsep, out _);
				Debug.Assert(r);
				Debug.Assert(!separator.Bytes.SequenceEqual(hsep.Bytes));

				Span<byte> hsepCopy = stackalloc byte[hsep.Bytes.Length];
				hsep.Bytes.CopyTo(hsepCopy);

				Transaction.Save(target);

				var tracebackCopy = traceback.Clone();
				r = tracebackCopy.TryMoveUp();
				Debug.Assert(r);

				// Update the parent if the removed separator was the highest
				if (hsepCopy.SequenceCompareTo(separator.Bytes) < 0)
				{
					UpdateSeparatorPropagate(
						separator, NormalisedValueSpan.FromNormalised(hsepCopy),
						tracebackCopy
					);
				}

				else
				{
					RemoveSeparatorPropagate(separator, tracebackCopy);
				}
			}

			else
			{
				Transaction.Save(target);
			}
		}

		protected T SplitLeaf(T target, T left, NormalisedValueSpan separator, BTreeIndexTraceback traceback)
		{
			target.Spill(left, fromHighest: false);

			var r = traceback.TryMoveUp();
			Debug.Assert(r);

			r = left.TryReadHighest(out var lhkey);
			Debug.Assert(r);
			Debug.Assert(!lhkey.Bytes.SequenceEqual(separator.Bytes));

			r = target.TryReadLowest(out var rlkey);
			Debug.Assert(r);
			Debug.Assert(!rlkey.Bytes.SequenceEqual(separator.Bytes));

			T leaf;
			var insertLeft = false;
			if (separator.Bytes.SequenceCompareTo(rlkey.Bytes) < 0)
			{
				// No update for the parents required, because we didn't insert the separator yet
				leaf = left;
				insertLeft = true;
			}

			else
			{
				r = target.TryReadHighest(out var rhkey);
				Debug.Assert(r);
				Debug.Assert(!rhkey.Bytes.SequenceEqual(separator.Bytes));

				if (separator.Bytes.SequenceCompareTo(rhkey.Bytes) > 0)
				{
					UpdateSeparatorPropagate(rhkey, separator, traceback.Clone());
				}

				leaf = target;
			}

			Transaction.Save(left);
			Transaction.Save(target);

			if (insertLeft && separator.Bytes.SequenceCompareTo(lhkey.Bytes) > 0)
			{
				InsertSeparator(separator, left.Header.Handle, traceback);
			}

			else
			{
				Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Bytes.Length];
				lhkey.Bytes.CopyTo(lhkeySepCopy);

				InsertSeparator(
					NormalisedValueSpan.FromNormalised(lhkeySepCopy), left.Header.Handle,
					traceback
				);
			}

			return leaf;
		}

		protected void BalanceLeaf(BTreeIndexTraceback traceback)
		{
			void _merge(T left, T right)
			{
				var a = left.TryReadHighest(out var lhkey);
				var b = right.TryReadHighest(out var rhkey);
				Debug.Assert(a);
				Debug.Assert(b);

				Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Bytes.Length];
				Span<byte> rhkeySepCopy = stackalloc byte[rhkey.Bytes.Length];
				lhkey.Bytes.CopyTo(lhkeySepCopy);
				rhkey.Bytes.CopyTo(rhkeySepCopy);

				right.Flush(left, fromHighest: false);

				var r = left.TryReadHighest(out var nlhkey);
				Debug.Assert(r);

				Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Bytes.Length];
				nlhkey.Bytes.CopyTo(nlhkeySepCopy);

				r = traceback.TryMoveUp();
				Debug.Assert(r);

				// Must be before deallocating right
				Transaction.Save(left);

				// Remove right's highest key from the parent before replacing left's separator with it
				if (!right.TryReadHighest(out _))
				{
					ChainHelpers.RemoveAndDeallocate(right, Transaction);
					RemoveSeparatorPropagate(NormalisedValueSpan.FromNormalised(rhkeySepCopy), traceback.Clone());
				}

				else
				{
					Transaction.Save(right);
				}

				UpdateSeparatorPropagate(
					NormalisedValueSpan.FromNormalised(lhkeySepCopy),
					NormalisedValueSpan.FromNormalised(nlhkeySepCopy),
					traceback
				);
			}

			var target = Transaction.Load<T>(traceback.Current);
			if (!target.IsUnderflowed)
			{
				return;
			}

			/* Before trying to take keys from the sibling or merge the two, 
			 * we make sure they belong to the same parent
			 */

			var r = traceback.TryPeekUp(out var parentHandle);
			Debug.Assert(r);

			if (!target.Next.IsNull)
			{
				var next = Transaction.Load<T>(target.Next);
				r = next.TryReadHighest(out var nhkey);
				Debug.Assert(r);

				if (!next.IsUnderflowed && _parentContains(parentHandle, nhkey))
				{
					r = target.TryReadHighest(out var lhkey);
					Debug.Assert(r);

					Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Bytes.Length];
					lhkey.Bytes.CopyTo(lhkeySepCopy);

					next.Spill(target, fromHighest: false);

					r = target.TryReadHighest(out var nlhkey);
					Debug.Assert(r);

					Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Bytes.Length];
					nlhkey.Bytes.CopyTo(nlhkeySepCopy);

					r = traceback.TryMoveUp();
					Debug.Assert(r);

					Transaction.Save(target);
					Transaction.Save(next);

					// Taking keys from the right sibling in ascending order changes left's highest key,
					// but not right's highest key, thus we only need to update left's separator in parent
					UpdateSeparatorPropagate(
						NormalisedValueSpan.FromNormalised(lhkeySepCopy),
						NormalisedValueSpan.FromNormalised(nlhkeySepCopy),
						traceback
					);
				}

				else
				if (next.IsUnderflowed && _parentContains(parentHandle, nhkey))
				{
					_merge(left: target, right: next);
				}
			}

			else
			if (!target.Previous.IsNull)
			{
				var previous = Transaction.Load<T>(target.Previous);
				r = previous.TryReadHighest(out var phkey);
				Debug.Assert(r);

				if (!previous.IsUnderflowed && _parentContains(parentHandle, phkey))
				{
					r = previous.TryReadHighest(out var lhkey);
					Debug.Assert(r);

					Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Bytes.Length];
					lhkey.Bytes.CopyTo(lhkeySepCopy);

					previous.Spill(target, fromHighest: true);

					r = previous.TryReadHighest(out var nlhkey);
					Debug.Assert(r);

					Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Bytes.Length];
					nlhkey.Bytes.CopyTo(nlhkeySepCopy);

					r = traceback.TryMoveUp();
					Debug.Assert(r);

					Transaction.Save(target);
					Transaction.Save(previous);

					// Same as above, except we take keys from the left sibling in descending order,
					// in order to preserve the correct order of keys in the tree
					UpdateSeparatorPropagate(
						NormalisedValueSpan.FromNormalised(lhkeySepCopy),
						NormalisedValueSpan.FromNormalised(nlhkeySepCopy),
						traceback
					);
				}

				else
				if (previous.IsUnderflowed && _parentContains(parentHandle, phkey))
				{
					_merge(left: previous, right: target);
				}
			}
		}

		protected void HandleBeforeSplit(T target, T left)
		{
			if (target.Previous.IsNull)
			{
				ChainHelpers.Prepend(left, target);
			}

			else
			{
				var previous = Transaction.Load<T>(target.Previous);
				ChainHelpers.Insert(left, previous, target);
				Transaction.Save(previous);
			}
		}

		protected void HandlePostRemoval(T target, NormalisedValueSpan separator, BTreeIndexTraceback traceback)
		{
			// Remove the leaf if it's empty
			if (!target.TryReadLowest(out _))
			{
				ChainHelpers.RemoveAndDeallocate(target, Transaction);

				var tracebackCopy = traceback.Clone();
				var r = tracebackCopy.TryMoveUp();
				Debug.Assert(r);

				RemoveSeparatorPropagate(separator, tracebackCopy);
			}

			else
			{
				var r = target.TryReadHighest(out var hsep);
				Debug.Assert(r);

				Span<byte> hsepCopy = stackalloc byte[hsep.Bytes.Length];
				hsep.Bytes.CopyTo(hsepCopy);

				Transaction.Save(target);

				// Update the parent if the removed separator was the highest
				if (hsep.Bytes.SequenceCompareTo(separator.Bytes) < 0)
				{
					var tracebackCopy = traceback.Clone();
					r = tracebackCopy.TryMoveUp();
					Debug.Assert(r);

					UpdateSeparatorPropagate(
						separator, NormalisedValueSpan.FromNormalised(hsepCopy), 
						tracebackCopy
					);
				}

				BalanceLeaf(traceback);
			}
		}

		private bool _parentContains(PageHandle parentHandle, NormalisedValueSpan separator)
		{
			var target = Transaction.Load<BTreePage>(parentHandle);
			if (target.TryReadSeparatorHandle(separator, out _))
			{
				Transaction.Save(target);
				return true;
			}

			return false;
		}
	}
}
