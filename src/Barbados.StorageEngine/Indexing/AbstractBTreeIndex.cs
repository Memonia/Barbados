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

		public PagePool Pool { get; }
		public BTreeIndexInfo Info { get; }

		protected AbstractBTreeIndex(PagePool pool, BTreeIndexInfo info)
		{
			if (info.KeyMaxLength <= 0 || info.KeyMaxLength > Constants.IndexKeyMaxLength)
			{
				throw new ArgumentException(
					$"Index key maximum length must be between 1 and {Constants.IndexKeyMaxLength} bytes"
				);
			}

			Info = info;
			Pool = pool;
		}

		protected void Deallocate()
		{
			void _deallocate(PageHandle handle)
			{
				if (
					!Pool.IsPageType(handle, PageMarker.BTreeRoot) &&
					!Pool.IsPageType(handle, PageMarker.BTreeNode)
				)
				{
					Pool.Deallocate(handle);
					return;
				}

				var node = Pool.LoadPin<BTreePage>(handle);
				var e = node.GetEnumerator();
				while (e.TryGetNext(out var separator))
				{
					var r = node.TryReadSeparatorHandle(separator, out var lessOrEqual);
					Debug.Assert(r);

					_deallocate(lessOrEqual);
				}

				Pool.Release(node);
				Pool.Deallocate(handle);
			}

			_deallocate(Info.RootPageHandle);
		}

		protected bool TryFind(BTreeIndexKey search, out BTreeIndexTraceback traceback)
		{
			var root = Pool.LoadPin<BTreeRootPage>(Info.RootPageHandle);
			var trace = new List<PageHandle>
			{
				Info.RootPageHandle
			};

			if (root.TryReadSubtreeHandle(search.Separator, out var subtreeHandle))
			{
				Pool.Release(root);
				trace.Add(subtreeHandle);
				while (Pool.IsPageType(subtreeHandle, PageMarker.BTreeNode))
				{
					var node = Pool.LoadPin<BTreePage>(subtreeHandle);
					var r = node.TryReadSubtreeHandle(search.Separator, out subtreeHandle);
					Debug.Assert(r);

					Pool.Release(node);
					trace.Add(subtreeHandle);
				}

				traceback = new(trace);
				return true;
			}

			else
			if (root.TryReadHighestSeparatorHandle(out _, out var lessOrEqual))
			{
				Pool.Release(root);
				trace.Add(lessOrEqual);
				while (Pool.IsPageType(lessOrEqual, PageMarker.BTreeNode))
				{
					var node = Pool.LoadPin<BTreePage>(lessOrEqual);
					var r = node.TryReadHighestSeparatorHandle(out _, out lessOrEqual);
					Debug.Assert(r);

					Pool.Release(node);
					trace.Add(lessOrEqual);
				}

				traceback = new(trace);
				return true;
			}

			Pool.Release(root);

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
				var node = Pool.LoadPin<BTreePage>(traceback.Current);
				if (node.CanFit(Info.KeyMaxLength * 2))
				{
					trace.Add(node.Header.Handle);
					Pool.Release(node);
				}

				else
				{
					if (node.Header.Marker == PageMarker.BTreeRoot)
					{
						var lh = Pool.Allocate();
						var rh = Pool.Allocate();
						var left = new BTreePage(lh);
						var right = new BTreePage(rh);

						_splitRoot(node, left, right);
						var next = _getHandleContainingSeparator(left, right, search.Separator);
						trace.Add(traceback.Current);
						trace.Add(next);

						Debug.Assert(node.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(left.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(right.CanFit(Info.KeyMaxLength * 2));

						Pool.Save(left);
						Pool.Save(right);
						Pool.SaveRelease(node);
					}

					else
					{
						/* Even though we write a new separator to the parent after the split, it's
						 * only going to happen once per node. Since we split preemptively to fit at least
						 * two keys, we can be sure there will be enough space in all nodes in traceback to
						 * fit a new separator from the split in the leaf node.
						 */

						var parent = Pool.LoadPin<BTreePage>(trace[^1]);
						var sh = Pool.Allocate();
						var split = new BTreePage(sh);

						_splitNode(parent, node, split);

						// Node gives up the lowest keys, see '_split'
						var next = _getHandleContainingSeparator(split, node, search.Separator);
						trace.Add(next);

						Debug.Assert(parent.CanFit(Info.KeyMaxLength));
						Debug.Assert(node.CanFit(Info.KeyMaxLength * 2));
						Debug.Assert(split.CanFit(Info.KeyMaxLength * 2));

						Pool.Save(split);
						Pool.SaveRelease(node);
						Pool.SaveRelease(parent);
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
