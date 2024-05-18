using System;
using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal abstract partial class AbstractBTreeIndex<T> where T : AbstractPage, IBTreeIndexLeaf<T>
	{
		/* Locks must be handled by derived classes 
		 */

		public BTreeIndexInfo Info { get; }
		public BarbadosController Controller { get; }

		protected AbstractBTreeIndex(BarbadosController controller, BTreeIndexInfo info)
		{
			if (info.KeyMaxLength <= 0 || info.KeyMaxLength > Constants.IndexKeyMaxLength)
			{
				throw new ArgumentException(
					$"Index key maximum length must be between 1 and {Constants.IndexKeyMaxLength} bytes"
				);
			}

			Controller = controller;
			Info = info;
		}

		protected void Deallocate()
		{
			void _deallocate(PageHandle handle)
			{
				if (
					!Controller.Pool.IsPageType(handle, PageMarker.BTreeRoot) &&
					!Controller.Pool.IsPageType(handle, PageMarker.BTreeNode)
				)
				{
					Controller.Pool.Deallocate(handle);
					return;
				}

				var node = Controller.Pool.LoadPin<BTreePage>(handle);
				var e = node.GetEnumerator();
				while (e.TryGetNext(out var separator))
				{
					var r = node.TryReadSeparatorHandle(separator, out var lessOrEqual);
					Debug.Assert(r);

					_deallocate(lessOrEqual);
				}

				Controller.Pool.Release(node);
				Controller.Pool.Deallocate(handle);
			}

			_deallocate(Info.RootPageHandle);
		}

		protected bool TryFind(BTreeIndexKey search, out BTreeIndexTraceback traceback)
		{
			var root = Controller.Pool.LoadPin<BTreeRootPage>(Info.RootPageHandle);
			var trace = new List<PageHandle>
			{
				Info.RootPageHandle
			};

			if (root.TryReadSubtreeHandle(search.Separator, out var subtreeHandle))
			{
				Controller.Pool.Release(root);
				trace.Add(subtreeHandle);
				while (Controller.Pool.IsPageType(subtreeHandle, PageMarker.BTreeNode))
				{
					var node = Controller.Pool.LoadPin<BTreePage>(subtreeHandle);
					var r = node.TryReadSubtreeHandle(search.Separator, out subtreeHandle);
					Debug.Assert(r);

					Controller.Pool.Release(node);
					trace.Add(subtreeHandle);
				}

				traceback = new(trace);
				return true;
			}

			else
			if (root.TryReadHighestSeparatorHandle(out _, out var lessOrEqual))
			{
				Controller.Pool.Release(root);
				trace.Add(lessOrEqual);
				while (Controller.Pool.IsPageType(lessOrEqual, PageMarker.BTreeNode))
				{
					var node = Controller.Pool.LoadPin<BTreePage>(lessOrEqual);
					var r = node.TryReadHighestSeparatorHandle(out _, out lessOrEqual);
					Debug.Assert(r);

					Controller.Pool.Release(node);
					trace.Add(lessOrEqual);
				}

				traceback = new(trace);
				return true;
			}

			Controller.Pool.Release(root);

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
				var node = Controller.Pool.LoadPin<BTreePage>(traceback.Current);
				if (node.CanFit(Info.KeyMaxLength * 2))
				{
					trace.Add(node.Header.Handle);
					Controller.Pool.Release(node);
				}

				else
				{
					if (node.Header.Marker == PageMarker.BTreeRoot)
					{
						var lh = Controller.Pool.Allocate();
						var rh = Controller.Pool.Allocate();
						var left = new BTreePage(lh);
						var right = new BTreePage(rh);

						_splitRoot(node, left, right);
						var next = _getHandleContainingSeparator(left, right, search.Separator);
						trace.Add(traceback.Current);
						trace.Add(next);

						Debug.Assert(node.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(left.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(right.CanFit(Info.KeyMaxLength * 2));

						Controller.Pool.SaveRelease(node);
						Controller.Pool.SaveRelease(left);
						Controller.Pool.SaveRelease(right);
					}

					else
					{
						/* Even though we write a new separator to the parent after the split, it's
						 * only going to happen once per node. Since we split preemptively to fit at least
						 * two keys, we can be sure there will be enough space in all nodes in traceback to
						 * fit a new separator from the split in the leaf node.
						 */

						var parent = Controller.Pool.LoadPin<BTreePage>(trace[^1]);
						var sh = Controller.Pool.Allocate();
						var split = new BTreePage(sh);

						_splitNode(parent, node, split);

						// Node gives up the lowest keys, see '_split'
						var next = _getHandleContainingSeparator(split, node, search.Separator);
						trace.Add(next);

						Debug.Assert(parent.CanFit(Info.KeyMaxLength));
						Debug.Assert(node.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(split.CanFit(Info.KeyMaxLength * 2));

						Controller.Pool.SaveRelease(node);
						Controller.Pool.SaveRelease(split);
						Controller.Pool.SaveRelease(parent);
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

		protected BTreeIndexKey ToBTreeIndexKey(NormalisedValue key)
		{
			var kb = key.AsSpan().Bytes;
			return kb.Length > Info.KeyMaxLength
				? new(NormalisedValueSpan.FromNormalised(kb[..Info.KeyMaxLength]), true)
				: new(new(key), false);
		}
	}
}
