using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Indexing
{
	internal abstract partial class AbstractBTreeIndexTransactionProxy<T> where T : AbstractPage, IBTreeIndexLeaf<T>
	{
		public BTreeIndexInfo Info { get; }
		public TransactionScope Transaction { get; }

		protected AbstractBTreeIndexTransactionProxy(TransactionScope transaction, BTreeIndexInfo info)
		{
			Info = info;
			Transaction = transaction;
		}

		public bool TryFind(BTreeIndexKey search, out BTreeIndexTraceback traceback)
		{
			var root = Transaction.Load<BTreePage>(Info.RootHandle);
			var trace = new List<PageHandle>
			{
				Info.RootHandle
			};

			if (root.TryReadSubtreeHandle(search.Separator, out var subtreeHandle))
			{
				trace.Add(subtreeHandle);
				while (Transaction.IsPageType(subtreeHandle, PageMarker.BTreeNode))
				{
					var node = Transaction.Load<BTreePage>(subtreeHandle);
					var r = node.TryReadSubtreeHandle(search.Separator, out subtreeHandle);
					Debug.Assert(r);
					trace.Add(subtreeHandle);
				}

				traceback = new(trace);
				return true;
			}

			else
			if (root.TryReadHighestSeparatorHandle(out _, out var lessOrEqual))
			{
				trace.Add(lessOrEqual);
				while (Transaction.IsPageType(lessOrEqual, PageMarker.BTreeNode))
				{
					var node = Transaction.Load<BTreePage>(lessOrEqual);
					var r = node.TryReadHighestSeparatorHandle(out _, out lessOrEqual);
					Debug.Assert(r);
					trace.Add(lessOrEqual);
				}

				traceback = new(trace);
				return true;
			}

			traceback = default!;
			return false;
		}

		protected bool TryFindWithPreemptiveSplit(BTreeIndexKey search, out BTreeIndexTraceback traceback)
		{
			static PageHandle _getHandleContainingSeparator(BTreePage left, BTreePage right, NormalisedValueSpan separator)
			{
				if (left.TryReadSubtreeHandle(separator, out _))
				{
					return left.Header.Handle;
				}

				return right.Header.Handle;
			}

			var trace = new List<PageHandle>();
			if (!TryFind(search, out traceback))
			{
				return false;
			}

			traceback.ResetTop();
			while (traceback.CanMoveDown)
			{
				var node = Transaction.Load<BTreePage>(traceback.Current);
				if (node.CanFit(Info.KeyMaxLength * 2))
				{
					trace.Add(node.Header.Handle);
				}

				else
				{
					if (node.Header.Handle.Handle == Info.RootHandle.Handle)
					{
						var lh = Transaction.AllocateHandle();
						var rh = Transaction.AllocateHandle();
						var left = new BTreePage(lh);
						var right = new BTreePage(rh);

						_splitRoot(node, left, right);
						var next = _getHandleContainingSeparator(left, right, search.Separator);
						trace.Add(traceback.Current);
						trace.Add(next);

						Debug.Assert(node.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(left.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(right.CanFit(Info.KeyMaxLength * 2));

						Transaction.Save(left);
						Transaction.Save(right);
						Transaction.Save(node);
					}

					else
					{
						/* Even though we write a new separator to the parent after the split, it's
						 * only going to happen once per node. Since we split preemptively to fit at least
						 * two keys, we can be sure there will be enough space in all nodes in traceback to
						 * fit a new separator from the split in the leaf node.
						 */

						var parent = Transaction.Load<BTreePage>(trace[^1]);
						var sh = Transaction.AllocateHandle();
						var split = new BTreePage(sh);

						_splitNode(parent, node, split);

						// Node gives up the lowest keys, see '_split'
						var next = _getHandleContainingSeparator(split, node, search.Separator);
						trace.Add(next);

						Debug.Assert(parent.CanFit(Info.KeyMaxLength));
						Debug.Assert(node.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(split.CanFit(Info.KeyMaxLength * 2));

						Transaction.Save(split);
						Transaction.Save(node);
						Transaction.Save(parent);
					}
				}

				var t = traceback.TryMoveDown();
				Debug.Assert(t);
			}

			// Last handle is the leaf node
			trace.Add(traceback.Current);
			traceback = new(trace);
			return true;
		}
	}
}
