using System;
using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.BTree.Pages;
using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.BTree
{
	internal partial class BTreeContext
	{
		private static void _splitRoot(BTreePage root, BTreePage left, BTreePage right)
		{
			root.Spill(right, fromHighest: true);
			root.Flush(left, fromHighest: true);
			Debug.Assert(root.Count == 0);

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

		private void _insertSeparator(BTreeNormalisedValueSpan separator, PageHandle lessOrEqual, BTreeLookupTraceback traceback)
		{
			var target = Transaction.Load<BTreePage>(traceback.Current);
			if (target.TryReadHighestSeparatorHandle(out var highest, out _) && separator.Bytes.SequenceCompareTo(highest.Bytes) > 0)
			{
				var tracebackCopy = traceback.Clone();
				if (tracebackCopy.TryMoveUp())
				{
					_updateSeparatorPropagate(highest, separator, traceback);
				}
			}

			// There must be enough space after preemptive splitting
			var r = target.TryWriteSeparatorHandle(separator, lessOrEqual);
			Debug.Assert(r);

			Transaction.Save(target);
		}

		private void _updateSeparatorPropagate(BTreeNormalisedValueSpan separator, BTreeNormalisedValueSpan replacement, BTreeLookupTraceback traceback)
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
				_updateSeparatorPropagate(separator, replacement, tracebackCopy);
			}

			var r = target.TryRemoveSeparatorHandle(separator);
			Debug.Assert(r);

			Transaction.Save(target);
			_insertSeparator(replacement, lessOrEqual, traceback);
		}

		private void _removeSeparatorPropagate(BTreeNormalisedValueSpan separator, BTreeLookupTraceback traceback)
		{
			var target = Transaction.Load<BTreePage>(traceback.Current);
			if (!target.TryRemoveSeparatorHandle(separator))
			{
				return;
			}

			// Remove an empty page from the tree, unless it is the root
			if (target.Count == 0 && traceback.CanMoveUp)
			{
				Transaction.Deallocate(target.Header.Handle);
				var r = traceback.TryMoveUp();
				Debug.Assert(r);
				_removeSeparatorPropagate(separator, traceback);
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
					_updateSeparatorPropagate(
						separator, BTreeNormalisedValueSpan.FromNormalised(hsepCopy),
						tracebackCopy
					);
				}

				else
				{
					_removeSeparatorPropagate(separator, tracebackCopy);
				}
			}

			else
			{
				Transaction.Save(target);
			}
		}

		private BTreeLeafPage _splitLeafPropagate(
			BTreeLeafPage target,
			BTreeLeafPage left,
			BTreeNormalisedValueSpan separator,
			BTreeLookupTraceback traceback
		)
		{
			target.Spill(left, fromHighest: false);

			var r = traceback.TryMoveUp();
			Debug.Assert(r);

			r = left.TryGetHighestKey(out var lhkey);
			Debug.Assert(r);
			Debug.Assert(!lhkey.Separator.Bytes.SequenceEqual(separator.Bytes));

			r = target.TryGetLowestKey(out var rlkey);
			Debug.Assert(r);
			Debug.Assert(!rlkey.Separator.Bytes.SequenceEqual(separator.Bytes));

			BTreeLeafPage leaf;
			var insertLeft = false;
			if (separator.Bytes.SequenceCompareTo(rlkey.Separator.Bytes) < 0)
			{
				// No update for the parents required, because we didn't insert the separator yet
				leaf = left;
				insertLeft = true;
			}

			else
			{
				r = target.TryGetHighestKey(out var rhkey);
				Debug.Assert(r);
				Debug.Assert(!rhkey.Separator.Bytes.SequenceEqual(separator.Bytes));

				if (separator.Bytes.SequenceCompareTo(rhkey.Separator.Bytes) > 0)
				{
					_updateSeparatorPropagate(rhkey.Separator, separator, traceback.Clone());
				}

				leaf = target;
			}

			Transaction.Save(left);
			Transaction.Save(target);

			if (insertLeft && separator.Bytes.SequenceCompareTo(lhkey.Separator.Bytes) > 0)
			{
				_insertSeparator(separator, left.Header.Handle, traceback);
			}

			else
			{
				Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Separator.Bytes.Length];
				lhkey.Separator.Bytes.CopyTo(lhkeySepCopy);

				_insertSeparator(
					BTreeNormalisedValueSpan.FromNormalised(lhkeySepCopy), left.Header.Handle,
					traceback
				);
			}

			return leaf;
		}

		private void _balanceLeaf(BTreeLookupTraceback traceback)
		{
			void _merge(BTreeLeafPage left, BTreeLeafPage right)
			{
				var a = left.TryGetHighestKey(out var lhkey);
				var b = right.TryGetHighestKey(out var rhkey);
				Debug.Assert(a);
				Debug.Assert(b);

				Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Separator.Bytes.Length];
				Span<byte> rhkeySepCopy = stackalloc byte[rhkey.Separator.Bytes.Length];
				lhkey.Separator.Bytes.CopyTo(lhkeySepCopy);
				rhkey.Separator.Bytes.CopyTo(rhkeySepCopy);

				right.Flush(left, fromHighest: false);

				var r = left.TryGetHighestKey(out var nlhkey);
				Debug.Assert(r);

				Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Separator.Bytes.Length];
				nlhkey.Separator.Bytes.CopyTo(nlhkeySepCopy);

				r = traceback.TryMoveUp();
				Debug.Assert(r);

				// Must be before deallocating right
				Transaction.Save(left);

				// Remove right's highest key from the parent before replacing left's separator with it
				if (!right.TryGetHighestKey(out _))
				{
					ChainHelpers.RemoveAndDeallocate(right, Transaction);
					_removeSeparatorPropagate(BTreeNormalisedValueSpan.FromNormalised(rhkeySepCopy), traceback.Clone());
				}

				else
				{
					Transaction.Save(right);
				}

				_updateSeparatorPropagate(
					BTreeNormalisedValueSpan.FromNormalised(lhkeySepCopy),
					BTreeNormalisedValueSpan.FromNormalised(nlhkeySepCopy),
					traceback
				);
			}

			var target = Transaction.Load<BTreeLeafPage>(traceback.Current);
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
				var next = Transaction.Load<BTreeLeafPage>(target.Next);
				r = next.TryGetHighestKey(out var nhkey);
				Debug.Assert(r);

				if (!next.IsUnderflowed && _parentContains(parentHandle, nhkey.Separator))
				{
					r = target.TryGetHighestKey(out var lhkey);
					Debug.Assert(r);

					Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Separator.Bytes.Length];
					lhkey.Separator.Bytes.CopyTo(lhkeySepCopy);

					next.Spill(target, fromHighest: false);

					r = target.TryGetHighestKey(out var nlhkey);
					Debug.Assert(r);

					Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Separator.Bytes.Length];
					nlhkey.Separator.Bytes.CopyTo(nlhkeySepCopy);

					r = traceback.TryMoveUp();
					Debug.Assert(r);

					Transaction.Save(target);
					Transaction.Save(next);

					// Taking keys from the right sibling in ascending order changes left's highest key,
					// but not right's highest key, thus we only need to update left's separator in parent
					_updateSeparatorPropagate(
						BTreeNormalisedValueSpan.FromNormalised(lhkeySepCopy),
						BTreeNormalisedValueSpan.FromNormalised(nlhkeySepCopy),
						traceback
					);
				}

				else
				if (next.IsUnderflowed && _parentContains(parentHandle, nhkey.Separator))
				{
					_merge(left: target, right: next);
				}
			}

			else
			if (!target.Previous.IsNull)
			{
				var previous = Transaction.Load<BTreeLeafPage>(target.Previous);
				r = previous.TryGetHighestKey(out var phkey);
				Debug.Assert(r);

				if (!previous.IsUnderflowed && _parentContains(parentHandle, phkey.Separator))
				{
					r = previous.TryGetHighestKey(out var lhkey);
					Debug.Assert(r);

					Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Separator.Bytes.Length];
					lhkey.Separator.Bytes.CopyTo(lhkeySepCopy);

					previous.Spill(target, fromHighest: true);

					r = previous.TryGetHighestKey(out var nlhkey);
					Debug.Assert(r);

					Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Separator.Bytes.Length];
					nlhkey.Separator.Bytes.CopyTo(nlhkeySepCopy);

					r = traceback.TryMoveUp();
					Debug.Assert(r);

					Transaction.Save(target);
					Transaction.Save(previous);

					// Same as above, except we take keys from the left sibling in descending order,
					// in order to preserve the correct order of keys in the tree
					_updateSeparatorPropagate(
						BTreeNormalisedValueSpan.FromNormalised(lhkeySepCopy),
						BTreeNormalisedValueSpan.FromNormalised(nlhkeySepCopy),
						traceback
					);
				}

				else
				if (previous.IsUnderflowed && _parentContains(parentHandle, phkey.Separator))
				{
					_merge(left: previous, right: target);
				}
			}
		}

		private void _handleBeforeSplit(BTreeLeafPage target, BTreeLeafPage left)
		{
			if (target.Previous.IsNull)
			{
				ChainHelpers.Prepend(left, target);
			}

			else
			{
				var previous = Transaction.Load<BTreeLeafPage>(target.Previous);
				ChainHelpers.Insert(left, previous, target);
				Transaction.Save(previous);
			}
		}

		private void _handlePostRemoval(
			BTreeLeafPage target,
			BTreeNormalisedValueSpan separator,
			BTreeLookupTraceback traceback
		)
		{
			// Remove the leaf if it's empty
			if (!target.TryGetLowestKey(out _))
			{
				ChainHelpers.RemoveAndDeallocate(target, Transaction);

				var tracebackCopy = traceback.Clone();
				var r = tracebackCopy.TryMoveUp();
				Debug.Assert(r);

				_removeSeparatorPropagate(separator, tracebackCopy);
			}

			else
			{
				var r = target.TryGetHighestKey(out var hsep);
				Debug.Assert(r);

				Span<byte> hsepCopy = stackalloc byte[hsep.Separator.Bytes.Length];
				hsep.Separator.Bytes.CopyTo(hsepCopy);

				Transaction.Save(target);

				// Update the parent if the removed separator was the highest
				if (hsep.Separator.Bytes.SequenceCompareTo(separator.Bytes) < 0)
				{
					var tracebackCopy = traceback.Clone();
					r = tracebackCopy.TryMoveUp();
					Debug.Assert(r);

					_updateSeparatorPropagate(
						separator, BTreeNormalisedValueSpan.FromNormalised(hsepCopy),
						tracebackCopy
					);
				}

				_balanceLeaf(traceback);
			}
		}

		private bool _parentContains(PageHandle parentHandle, BTreeNormalisedValueSpan separator)
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
