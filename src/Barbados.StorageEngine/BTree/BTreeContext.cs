using System;
using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.BTree.Pages;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.BTree
{
	internal sealed partial class BTreeContext
	{
		public const int WorstCaseOverheadPerLookupKey = ChunkKey.OverheadPerLookupKeyLength;

		public BTreeInfo Info { get; }
		public TransactionScope Transaction { get; }

		public BTreeContext(BTreeInfo info, TransactionScope transaction)
		{
			Info = info;
			Transaction = transaction;
		}

		public Enumerator GetDataEnumerator(BTreeFindOptions options)
		{
			return new(this, options);
		}

		public void Deallocate()
		{
			void _deallocate(PageHandle handle)
			{
				if (
					handle.Handle != Info.RootHandle.Handle &&
					!Transaction.IsPageType(handle, PageMarker.BTreeNode)
				)
				{
					Transaction.Deallocate(handle);
					return;
				}

				var node = Transaction.Load<BTreePage>(handle);
				var e = node.GetEnumerator();
				while (e.TryGetNext(out var separator))
				{
					var r = node.TryReadSeparatorHandle(separator, out var lessOrEqual);
					Debug.Assert(r);

					_deallocate(lessOrEqual);
				}

				Transaction.Deallocate(handle);
			}

			_deallocate(Info.RootHandle);
		}

		public bool TryFind(BTreeNormalisedValue key, out byte[] data)
		{
			var e = GetDataEnumerator(BTreeFindOptions.CreateFindSingle(key));
			if (e.MoveNext())
			{
				if (!e.TryGetCurrentDataAsSpan(out var span))
				{
					if (!e.TryGetCurrentData(out var arr))
					{
						throw BarbadosInternalErrorExceptionHelpers.CouldNotRetrieveDataFromEnumeratorAfterMoveNext();
					}

					data = arr;
				}

				else
				{
					data = span.ToArray();
				}

				return true;
			}

			data = default!;
			return false;
		}

		public bool TryInsert(BTreeNormalisedValueSpan key, ReadOnlySpan<byte> data)
		{
			var ik = new InternalLookupKeySpan(key, false);
			return _tryInsert(ik, data);
		}

		public bool TryRemove(BTreeNormalisedValueSpan key)
		{
			var rk = new InternalLookupKeySpan(key, false);
			return _tryRemove(rk);
		}

		private BTreeLookupKeySpan _toLookupKey(BTreeNormalisedValueSpan key, out ReadOnlySpan<byte> remainder)
		{
			var kb = key.Bytes;
			if (kb.Length > Info.MaxLookupKeyLength)
			{
				remainder = kb[Info.MaxLookupKeyLength..];
				return new(BTreeNormalisedValueSpan.FromNormalised(kb[..Info.MaxLookupKeyLength]), true);
			}

			else
			{
				remainder = [];
				return new(key, false);
			}
		}

		private BTreeLookupKeySpan _toLookupKey(InternalLookupKeySpan key, out ReadOnlySpan<byte> remainder)
		{
			if (!key.IsChunkKey)
			{
				return _toLookupKey(key.Separator, out remainder);
			}

			remainder = [];
			return new(key.Separator, false);
		}

		private bool _tryFind(BTreeLookupKeySpan search, out BTreeLookupTraceback traceback)
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

			// If we're here, then the key we're looking for is the biggest in the tree and so
			// it must be on the rightmost or leftmost leaf of the tree (depending on the tree order)
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

		private bool _tryFindWithPreemptiveSplit(BTreeLookupKeySpan search, out BTreeLookupTraceback traceback)
		{
			static PageHandle _getHandleContainingSeparator(BTreePage left, BTreePage right, BTreeNormalisedValueSpan separator)
			{
				if (left.TryReadSubtreeHandle(separator, out _))
				{
					return left.Header.Handle;
				}

				return right.Header.Handle;
			}

			var trace = new List<PageHandle>();
			if (!_tryFind(search, out traceback))
			{
				return false;
			}

			traceback.ResetTop();
			while (traceback.CanMoveDown)
			{
				var node = Transaction.Load<BTreePage>(traceback.Current);

				// Ensure that each node down the path can fit at least 2 keys:
				// one for the split separator
				// one for the key that we might attempt to insert
				if (node.CanFit(Info.MaxKeyDataLength * 2))
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

						Debug.Assert(node.CanFit(Info.MaxKeyDataLength * 2));
						Debug.Assert(left.CanFit(Info.MaxKeyDataLength * 2));
						Debug.Assert(right.CanFit(Info.MaxKeyDataLength * 2));

						Transaction.Save(left);
						Transaction.Save(right);
						Transaction.Save(node);
					}

					else
					{
						var parent = Transaction.Load<BTreePage>(trace[^1]);
						var sh = Transaction.AllocateHandle();
						var split = new BTreePage(sh);

						_splitNode(parent, node, split);

						// A node gives up the lowest keys, see '_split'
						var next = _getHandleContainingSeparator(split, node, search.Separator);
						trace.Add(next);

						Debug.Assert(parent.CanFit(Info.MaxKeyDataLength));
						Debug.Assert(node.CanFit(Info.MaxKeyDataLength * 2));
						Debug.Assert(split.CanFit(Info.MaxKeyDataLength * 2));

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

		private bool _tryInsert(InternalLookupKeySpan key, ReadOnlySpan<byte> data)
		{
			var lookupKey = _toLookupKey(key, out var lookupKeyRemainder);
			if (_tryFindWithPreemptiveSplit(lookupKey, out var traceback))
			{
				var leaf = Transaction.Load<BTreeLeafPage>(traceback.Current);

				// In case the leaf has been split, parent node updating has been handled already
				var splitLeaf = false;

				// When we need to insert chunked data we first update the leaf and propagate its
				// separator changes up the tree, so that when chunks are inserted, the algorithm
				// has most up-to-date tree view. Otherwise, 'hlkeySepCopy' below is useless, since
				// inserting chunks might have changed the current leaf we're working on, which erases
				// the information about leaf's keys at the time of insertion
				var chunkInsert = false;
				var chunkSequenceGapNumber = (long)-1;

				scoped Span<byte> hlkeySepCopy;
				if (!leaf.TryGetHighestKey(out var hlkey))
				{
					var minBytes = BTreeNormalisedValue.Min.AsSpan().Bytes;
					hlkeySepCopy = stackalloc byte[minBytes.Length];
					minBytes.CopyTo(hlkeySepCopy);
				}

				else
				{
					hlkeySepCopy = stackalloc byte[hlkey.Separator.Bytes.Length];
					hlkey.Separator.Bytes.CopyTo(hlkeySepCopy);
				}

				// Try to contain data on a single page
				if ((long)data.Length + key.Separator.Bytes.Length <= Info.MaxKeyDataLength && !lookupKey.IsTrimmed)
				{
					if (leaf.Exists(lookupKey))
					{
						return false;
					}

					if (!leaf.TryWriteData(lookupKey, data))
					{
						var insertTarget = _splitPropagate(leaf, lookupKey, traceback.Clone());
						var r = insertTarget.TryWriteData(lookupKey, data);
						Debug.Assert(r);

						splitLeaf = true;
						Transaction.Save(insertTarget);
					}

					else
					{
						Transaction.Save(leaf);
					}
				}

				// When data doesn't fit on a single page, we split it into addressable chunks.
				// Chunks of data and key remainder are inserted into the tree as separate items
				// with special keys. A set of chunks of an overflowed entry is called a sequence.
				// Each unique key might have at most two sequences: for data and key remainder,
				// but never zero
				else
				{
					// Has given key already been inserted?
					if (_tryGetKeySequenceNumber(key.Separator, out _, out chunkSequenceGapNumber))
					{
						return false;
					}

					// We can reuse sequence numbers from already deleted chunked keys of the same prefix.
					// If a gap is found, we can insert a new sequence with no bookkeeping required.
					// Otherwise, we need to update the overflow info to reflect the changes
					if (chunkSequenceGapNumber < 0)
					{
						// There were no gaps. If overflow info exists, then it's not the first sequence with this prefix.
						// Current sequence gets assigned a number and the overflow entry is updated to reflect that
						if (leaf.TryReadOverflowInfo(lookupKey, out var info))
						{
							if (info.NextSequenceNumber == long.MaxValue)
							{
								throw new BarbadosException(BarbadosExceptionCode.MaxSamePrefixKeyCountReached);
							}

							var r = leaf.TryRemoveOverflowInfo(lookupKey, out _);
							Debug.Assert(r);

							chunkSequenceGapNumber = info.NextSequenceNumber;

							// 'sequenceCount' is always <= 'nextSequenceNumber', so overflow check on 'nextSequenceNumber' is enough
							info = new BTreeLeafPage.OverflowInfo(
								sequenceCount: info.SequenceCount + 1,
								nextSequenceNumber: chunkSequenceGapNumber + 1
							);

							r = leaf.TryWriteOverflowInfo(lookupKey, info);
							Debug.Assert(r);

							Transaction.Save(leaf);
						}

						// This is the first sequence with current prefix
						else
						{
							chunkSequenceGapNumber = 0;
							info = new BTreeLeafPage.OverflowInfo(
								sequenceCount: 1,
								nextSequenceNumber: chunkSequenceGapNumber + 1
							);

							if (!leaf.TryWriteOverflowInfo(lookupKey, info))
							{
								var insertTarget = _splitPropagate(leaf, lookupKey, traceback.Clone());
								var r = insertTarget.TryWriteOverflowInfo(lookupKey, info);
								Debug.Assert(r);

								splitLeaf = true;
								Transaction.Save(insertTarget);
							}

							else
							{
								Transaction.Save(leaf);
							}
						}
					}

					chunkInsert = true;
				}

				// If inserted key is the new maximum, update the parent separators
				if (!splitLeaf && hlkeySepCopy.SequenceCompareTo(lookupKey.Separator.Bytes) < 0)
				{
					var r = traceback.TryMoveUp();
					Debug.Assert(r);

					_updateSeparatorPropagate(
						BTreeNormalisedValueSpan.FromNormalised(hlkeySepCopy), lookupKey.Separator,
						traceback
					);
				}

				if (chunkInsert)
				{
					// Insert data chunks first to save one parent update
					_writeChunkedData(lookupKey, chunkSequenceGapNumber, data, ChunkType.DataChunk);
					if (lookupKeyRemainder.Length > 0)
					{
						_writeChunkedData(lookupKey, chunkSequenceGapNumber, lookupKeyRemainder, ChunkType.KeyChunk);
					}
				}

				return true;
			}

			// BTree with no elements consists only of a root page
			else
			{
				var root = Transaction.Load<BTreePage>(Info.RootHandle);
				Debug.Assert(root.Count == 0);

				var lh = Transaction.AllocateHandle();
				var leaf = new BTreeLeafPage(lh);

				var r = root.TryWriteSeparatorHandle(lookupKey.Separator, leaf.Header.Handle);
				Debug.Assert(r);

				Transaction.Save(leaf);
				Transaction.Save(root);
				return _tryInsert(key, data);
			}
		}

		private bool _tryRemove(InternalLookupKeySpan key)
		{
			var lookupKey = _toLookupKey(key, out _);
			if (!_tryFind(lookupKey, out var traceback))
			{
				return false;
			}

			var leaf = Transaction.Load<BTreeLeafPage>(traceback.Current);
			if (!leaf.TryRemoveData(lookupKey))
			{
				if (!leaf.TryRemoveOverflowInfo(lookupKey, out var info))
				{
					return false;
				}

				Debug.Assert(info.SequenceCount > 0);
				if (!_tryGetKeySequenceNumber(key.Separator, out var sequenceNumber, out _))
				{
					return false;
				}

				// Do nothing if we're removing the last sequence, since the overflow info was removed above.
				// Otherwise, update the overflow info to reflect the changes
				if (info.SequenceCount > 1)
				{
					info = new BTreeLeafPage.OverflowInfo(
						sequenceCount: info.SequenceCount - 1,
						nextSequenceNumber: info.NextSequenceNumber
					);

					var r = leaf.TryWriteOverflowInfo(lookupKey, info);
					Debug.Assert(r);
				}

				Transaction.Save(leaf);
				_handlePostRemoval(leaf, lookupKey.Separator, traceback);

				// First remove key chunks to save one parent update
				if (lookupKey.IsTrimmed)
				{
					_removeChunkedData(lookupKey, sequenceNumber, ChunkType.KeyChunk);
				}
				_removeChunkedData(lookupKey, sequenceNumber, ChunkType.DataChunk);
			}

			else
			{
				Transaction.Save(leaf);
				_handlePostRemoval(leaf, lookupKey.Separator, traceback);
			}

			return true;
		}

		private BTreeLeafPage _splitPropagate(BTreeLeafPage leaf, BTreeLookupKeySpan key, BTreeLookupTraceback traceback)
		{
			Debug.Assert(leaf.Header.Handle.Handle == traceback.Current.Handle);
			var target = leaf;
			var lh = Transaction.AllocateHandle();
			var left = new BTreeLeafPage(lh);

			_handleBeforeSplit(target, left);
			var insertTarget = _splitLeafPropagate(target, left, key.Separator, traceback.Clone());
			var r = insertTarget.TryGetHighestKey(out var hkey);
			Debug.Assert(r);

			Span<byte> insertTargetHighestKeyCopy = stackalloc byte[hkey.Separator.Bytes.Length];
			hkey.Separator.Bytes.CopyTo(insertTargetHighestKeyCopy);

			// If to-be-inserted key is the new maximum, update the parent separators
			if (insertTargetHighestKeyCopy.SequenceCompareTo(key.Separator.Bytes) < 0)
			{
				r = traceback.TryMoveUp();
				Debug.Assert(r);

				_updateSeparatorPropagate(
					BTreeNormalisedValueSpan.FromNormalised(insertTargetHighestKeyCopy), key.Separator,
					traceback
				);
			}

			Transaction.Save(left);
			Transaction.Save(target);
			return insertTarget;
		}
	}
}
