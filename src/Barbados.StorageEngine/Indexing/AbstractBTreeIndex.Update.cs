using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class AbstractBTreeIndex<T>
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
			var target = Controller.Pool.LoadPin<BTreePage>(traceback.Current);

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

			Controller.Pool.SaveRelease(target);
		}

		protected void UpdateSeparatorPropagate(NormalisedValueSpan separator, NormalisedValueSpan replacement, BTreeIndexTraceback traceback)
		{
			var target = Controller.Pool.LoadPin<BTreePage>(traceback.Current);
			if (target.TryReadSeparatorHandle(separator, out var lessOrEqual))
			{
				var tracebackCopy = traceback.Clone();
				if (tracebackCopy.TryMoveUp())
				{
					UpdateSeparatorPropagate(separator, replacement, tracebackCopy);
				}

				var r = target.TryRemoveSeparatorHandle(separator);
				Debug.Assert(r);

				Controller.Pool.SaveRelease(target);
				InsertSeparator(replacement, lessOrEqual, traceback);
			}

			else
			{
				Controller.Pool.Release(target);
			}
		}

		protected void RemoveSeparatorPropagate(NormalisedValueSpan separator, BTreeIndexTraceback traceback)
		{
			var target = Controller.Pool.LoadPin<BTreePage>(traceback.Current);
			if (target.TryRemoveSeparatorHandle(separator))
			{
				// Remove an empty page from the tree, unless it is the root
				if (target.Count() == 0 && traceback.CanMoveUp)
				{
					Controller.Pool.Release(target);
					Controller.Pool.Deallocate(target.Header.Handle);

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

					Controller.Pool.SaveRelease(target);

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

					_balance(traceback);
				}

				else
				{
					Controller.Pool.SaveRelease(target);
				}
			}

			else
			{
				Controller.Pool.Release(target);
			}
		}

		protected void BalanceLeaf(BTreeIndexTraceback traceback)
		{
			void _merge(T left, T right)
			{
				var a = left.TryReadHighest(out var lhkey);
				var b = right.TryReadHighest(out var rhkey);
				Debug.Assert(a);
				Debug.Assert(b);

				Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Separator.Bytes.Length];
				Span<byte> rhkeySepCopy = stackalloc byte[rhkey.Separator.Bytes.Length];
				lhkey.Separator.Bytes.CopyTo(lhkeySepCopy);
				rhkey.Separator.Bytes.CopyTo(rhkeySepCopy);

				right.Flush(left, fromHighest: false);

				var r = left.TryReadHighest(out var nlhkey);
				Debug.Assert(r);

				Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Separator.Bytes.Length];
				nlhkey.Separator.Bytes.CopyTo(nlhkeySepCopy);

				r = traceback.TryMoveUp();
				Debug.Assert(r);

				// Must be before deallocating right
				Controller.Pool.SaveRelease(left);

				// Remove right's highest key from the parent before replacing left's separator with it
				if (!right.TryReadHighest(out _))
				{
					ChainHelpers.RemoveAndDeallocate(right, Controller.Pool);
					RemoveSeparatorPropagate(
						NormalisedValueSpan.FromNormalised(rhkeySepCopy),
						traceback.Clone()
					);
				}

				else
				{
					Controller.Pool.SaveRelease(right);
				}

				UpdateSeparatorPropagate(
					NormalisedValueSpan.FromNormalised(lhkeySepCopy),
					NormalisedValueSpan.FromNormalised(nlhkeySepCopy),
					traceback
				);
			}

			var target = Controller.Pool.LoadPin<T>(traceback.Current);
			if (!target.IsUnderflowed)
			{
				Controller.Pool.Release(target);
				return;
			}

			/* Before trying to take keys from the sibling or merge the two, 
			 * we make sure they belong to the same parent
			 */

			var r = traceback.TryPeekUp(out var parentHandle);
			Debug.Assert(r);

			if (!target.Next.IsNull)
			{
				var next = Controller.Pool.LoadPin<T>(target.Next);
				r = next.TryReadHighest(out var nhkey);
				Debug.Assert(r);

				if (!next.IsUnderflowed && _parentContains(parentHandle, nhkey.Separator))
				{
					r = target.TryReadHighest(out var lhkey);
					Debug.Assert(r);

					Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Separator.Bytes.Length];
					lhkey.Separator.Bytes.CopyTo(lhkeySepCopy);

					next.Spill(target, fromHighest: false);

					r = target.TryReadHighest(out var nlhkey);
					Debug.Assert(r);

					Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Separator.Bytes.Length];
					nlhkey.Separator.Bytes.CopyTo(nlhkeySepCopy);

					r = traceback.TryMoveUp();
					Debug.Assert(r);

					Controller.Pool.SaveRelease(target);
					Controller.Pool.SaveRelease(next);

					// Taking keys from the right sibling in ascending order changes left's highest key,
					// but not right's highest key, thus we only need to update left's separator in parent
					UpdateSeparatorPropagate(
						NormalisedValueSpan.FromNormalised(lhkeySepCopy),
						NormalisedValueSpan.FromNormalised(nlhkeySepCopy),
						traceback
					);
				}

				else
				if (next.IsUnderflowed && _parentContains(parentHandle, nhkey.Separator))
				{
					_merge(left: target, right: next);
				}

				else
				{
					Controller.Pool.Release(next);
					Controller.Pool.Release(target);
				}
			}

			else
			if (!target.Previous.IsNull)
			{
				var previous = Controller.Pool.LoadPin<T>(target.Previous);
				r = previous.TryReadHighest(out var phkey);
				Debug.Assert(r);

				if (!previous.IsUnderflowed && _parentContains(parentHandle, phkey.Separator))
				{
					r = previous.TryReadHighest(out var lhkey);
					Debug.Assert(r);

					Span<byte> lhkeySepCopy = stackalloc byte[lhkey.Separator.Bytes.Length];
					lhkey.Separator.Bytes.CopyTo(lhkeySepCopy);

					previous.Spill(target, fromHighest: true);

					r = previous.TryReadHighest(out var nlhkey);
					Debug.Assert(r);

					Span<byte> nlhkeySepCopy = stackalloc byte[nlhkey.Separator.Bytes.Length];
					nlhkey.Separator.Bytes.CopyTo(nlhkeySepCopy);

					r = traceback.TryMoveUp();
					Debug.Assert(r);

					Controller.Pool.SaveRelease(target);
					Controller.Pool.SaveRelease(previous);

					// Same as above, except we take keys from the left sibling in descending order,
					// in order to preserve the correct order of keys in the tree
					UpdateSeparatorPropagate(
						NormalisedValueSpan.FromNormalised(lhkeySepCopy),
						NormalisedValueSpan.FromNormalised(nlhkeySepCopy),
						traceback
					);
				}

				else
				if (previous.IsUnderflowed && _parentContains(parentHandle, phkey.Separator))
				{
					_merge(left: previous, right: target);
				}

				else
				{
					Controller.Pool.Release(previous);
					Controller.Pool.Release(target);
				}
			}

			else
			{
				Controller.Pool.Release(target);
			}
		}

		private void _balance(BTreeIndexTraceback traceback)
		{
			// Can't rebalance the root
			if (!traceback.CanMoveUp)
			{
				return;
			}

			var target = Controller.Pool.LoadPin<BTreePage>(traceback.Current);
			if (!target.IsUnderflowed)
			{
				Controller.Pool.Release(target);
				return;
			}

			Controller.Pool.Release(target);
			// Balancing goes here
		}

		private bool _parentContains(PageHandle parentHandle, NormalisedValueSpan separator)
		{
			var target = Controller.Pool.LoadPin<BTreePage>(parentHandle);
			if (target.TryReadSeparatorHandle(separator, out _))
			{
				Controller.Pool.SaveRelease(target);
				return true;
			}

			Controller.Pool.Release(target);
			return false;
		}
	}
}
