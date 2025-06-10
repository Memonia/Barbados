using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

using Barbados.StorageEngine.Caching;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Wal.Pages;

namespace Barbados.StorageEngine.Storage.Wal
{
	internal sealed partial class WalBuffer
	{
		public static void WriteWalHeader(IStorageWrapper wal, ulong fileMagic)
		{
			var header = new WalHeader(fileMagic);
			var buffer = new byte[Constants.WalHeaderLength];
			header.WriteTo(buffer);
			wal.Write(0, buffer);
		}

		public static void WriteWalHeader(IStorageWrapper wal, IStorageWrapper db)
		{
			var buffer = new PageBuffer();
			var r = db.Read(0, buffer.AsSpan());
			if (r < Constants.PageLength)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.UnexpectedEndOfFile, "Could not read the database header"
				);
			}

			var root = new RootPage(buffer);
			WriteWalHeader(wal, root.FileMagic);
		}

		/* Concurrency:
		 *  
		 * WalBuffer assumes several things about transactions:
		 *  1. Single writer or multiple readers per data structure, as was stated above
		 *  2. A single transaction cannot be executed concurrently by different threads
		 *  3. Transaction ids are monotonically increasing positive integers
		 *  
		 * Having this knowledge greatly simplifies the implementation.
		 *  
		 *  
		 * Recovery:
		 * 
		 * Saved pages are buffered in memory.
		 * When the number of pages exceeds specified treshold, all pages are written to WAL.
		 * 
		 * On commit, currently buffered pages are written to disk, commit record is appended and changes are persisted.
		 * If there are no ongoing transactions afterwards, a checkpoint is performed for every committed transaction in 
		 * WAL, after which it is truncated.
		 * 
		 * On startup a restore operation is performed. All pages of committed transactions in WAL are transfered to the
		 * main database file. Once restore is complete, the WAL file is truncated.
		 * 
		 * A checkpoint only occurs when there are no ongoing transactions. To prevent WAL from growing indefinitely,
		 * when the page count reaches the treshold, taking new snapshots is blocked until a checkpoint is performed.
		 * 
		 * 
		 * Caching and isolation:
		 * 
		 * As the engine operates, pages become spread between 
		 * the WAL buffer, WAL file, database cache and the database file.
		 * When a transaction requests a page, WAL buffer and WAL file are first searched to find the
		 * latest version of the requested page. If the page is not found, we need to look it up in cache.
		 * If the page is found in cache, we need to check if it's the latest availble version. Here, we use
		 * the knowledge that no data structures are modified concurrently (single writer - multiple readers),
		 * with the exception of allocation pages and the root. Since no other transaction can modify the page
		 * while the current transaction is running in write mode, we can just check whether the latest commit 
		 * id at the time of caching the page matches current transaction's latest commit id at the time of 
		 * taking the snapshot. If it does, the page can be returned. If it doesn't, then it is possibly outdated,
		 * in which case we need to check whether the page was committed in WAL file. If it was, we take it from there,
		 * if it wasn't, then the last place where the page can be is the main database file. Whether it's taken from
		 * the wal or from the database file, we cache it again with a latest commit id of current transaction's snapshot.
		 * 
		 * When the pages are taken from WAL buffer or cache, the copies are made in order to protect pages from
		 * other transactions. Even though we know that there is only a single writer at a time, making copies
		 * ensures that already cached pages will not be affected by malfunctioning transactions. A transaction can
		 * fail or a read-only transaction can modify loaded page (this will not be detected until after the 'Save'
		 * method is called).
		 * 
		 * 
		 * Allocation:
		 * 
		 * Even though allocations are performed within this class, for the sake of explanation,
		 * we will call this part an allocator.
		 * 
		 * Pages are a shared resource. Allocation chain together with root is a shared data structure,
		 * which can be modified concurrently by multiple transactions. 
		 * Transactions want to allocate and deallocate pages, but they still need to be isolated from
		 * each other. We can't allow two transactions to allocate the same page or to have allocated pages
		 * not returned to the pool of free pages if the transaction fails.
		 * 
		 * To achieve that, allocator is assigned a "virtual" transaction. It is called a virtual transaction, 
		 * because it has a snapshot and can use WalBuffer's Save and Load methods, but it doesn't commit or 
		 * rollback. This transaction only exists within WalBuffer and is used to leverage WAL functionality
		 * (allocation pages are buffered until the buffer limit is reached and are also cached). 
		 * 
		 * A checkpoint of all committed transactions serves as a commit for this virtual transaction, at which
		 * point a new snapshot is taken with the latest commit id. Allocator uses Load and Save to get and save
		 * its pages. Since this transaction persists from checkpoint to checkpoint, allocator has a consistent view
		 * of all allocations and deallocations. 
		 * 
		 * When multiple transactions allocate or deallocate pages, the allocator coordinates them and saves
		 * the global allocation information within its transaction. When a transaction is commited, the allocator
		 * moves pages from its transaction into the committed transaction ('Save' is called with committed
		 * transaction's snapshot). 
		 * 
		 * With this approach allocated pages are never lost if a transaction is rolledback. Allocated pages
		 * will become free pages immediately. It does mean that allocations are not fully side-effect free
		 * though
		 */

		private readonly WalHeader _header;
		private readonly IStorageWrapper _db;
		private readonly IStorageWrapper _wal;
		private readonly int _walPageCountLimit;
		private readonly int _bufferedPageCountLimit;
		private readonly Lock _sync;
		private readonly Lock _allocatorSync;
		private readonly ManualResetEventSlim _forceCheckpointEvent;
		private readonly ObjectId _allocatorTransactionId;
		private readonly ICache<PageHandle, CachedPageInfo> _cache;
		private readonly ConcurrentDictionary<ObjectId, WalTransactionInfo> _transactions;
		private readonly ConcurrentDictionary<PageHandle, long> _onDiskCommittedPageBuffers;

		private bool _restored;
		private int _walPageCount;
		private int _bufferedPageCount;
		private long _currentWalOffset;
		private ObjectId _latestCommitId;
		private Snapshot _allocatorSnapshot;
		private WalTransactionInfo _allocatorTransactionInfo;

		public WalBuffer(
			IStorageWrapper db,
			IStorageWrapper wal,
			CacheFactory cacheFactory,
			int walPageCountLimit,
			int bufferedPageCountLimit
		)
		{
			_db = db;
			_wal = wal;
			_cache = cacheFactory.GetCache<PageHandle, CachedPageInfo>();
			_walPageCountLimit = walPageCountLimit;
			_bufferedPageCountLimit = bufferedPageCountLimit;
			_sync = new();
			_allocatorSync = new();
			_forceCheckpointEvent = new(true);
			_allocatorTransactionId = new(-1);
			_transactions = [];
			_onDiskCommittedPageBuffers = [];
			_walPageCount = 0;
			_bufferedPageCount = 0;
			_currentWalOffset = Constants.WalHeaderLength;
			_latestCommitId = new(ObjectId.Invalid.Value + 1);
			_allocatorSnapshot = _takeSnapshot(_allocatorTransactionId);
			_allocatorTransactionInfo = new WalTransactionInfo(_allocatorSnapshot);

			var rootBuffer = _readPageBufferFromDb(PageHandle.Root);
			RootPage.ThrowDatabaseDoesNotExist(rootBuffer);
			RootPage.ThrowDatabaseVersionMismatch(rootBuffer);

			var headerBuffer = new byte[Constants.WalHeaderLength];
			var r = _wal.Read(0, headerBuffer);
			if (r < Constants.WalHeaderLength)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.UnexpectedEndOfFile, "Could not read the WAL header"
				);
			}
			WalHeader.ThrowWalDoesNotExist(headerBuffer);
			WalHeader.ThrowWalVersionMismatch(headerBuffer);
			_header = new WalHeader(headerBuffer);

			var root = new RootPage(rootBuffer);
			if (root.FileMagic != _header.FileMagic)
			{
				throw new BarbadosException(BarbadosExceptionCode.DbMagicWalMagicMismatch,
					"Database file magic does not match the WAL file magic. WAL file does not belong to this database"
				);
			}
		}

		public Snapshot TakeSnapshot(ObjectId transactionId)
		{
			_forceCheckpointEvent.Wait();
			lock (_sync)
			{
				var sp = _takeSnapshot(transactionId);
				if (!_transactions.TryAdd(transactionId, new WalTransactionInfo(sp)))
				{
					throw new BarbadosInternalErrorException();
				}

				return sp;
			}
		}

		public void Save(Snapshot snapshot, AbstractPage page)
		{
			var info = _getTransactionInfo(snapshot);
			var copy = new PageBuffer();
			var buffer = page.UpdateAndGetBuffer();

			buffer.AsSpan().CopyTo(copy.AsSpan());
			AbstractPage.WriteChecksum(copy);

			lock (_sync)
			{
				// Write all buffered pages to disk if the max count is reached
				if (_bufferedPageCount >= _bufferedPageCountLimit)
				{
					_flushAll();
				}

				if (info.PageBuffers.TryAdd(page.Header.Handle, copy))
				{
					_bufferedPageCount += 1;
				}

				else
				{
					info.PageBuffers[page.Header.Handle] = copy;
				}
			}
		}
		
		public void Commit(Snapshot snapshot)
		{
			lock (_sync)
			{
				var info = _getTransactionInfo(snapshot);

				// Saves allocation pages and possibly the root for transaction
				_saveAllocationChainUpdates(info);

				// Write buffered pages to WAL
				_flush(info, addToCommittedList: true);

				// Add already written pages to the list of committed pages
				foreach (var (handle, offset) in info.OnDiskPageBuffers)
				{
					_onDiskCommittedPageBuffers.AddOrUpdate(handle, offset, (_, _) => offset);
				}

				// Finally, append a commit record
				Span<byte> rb = stackalloc byte[Constants.WalRecordLength];
				var commit = new WalRecord(WalRecordTypeMarker.Commit, snapshot.TransactionId);
				commit.WriteTo(rb);
				_wal.Write(_currentWalOffset, rb);
				_currentWalOffset += Constants.WalRecordLength;

				_wal.Persist();
				_incrementCommitId();

				if (_walPageCount >= _walPageCountLimit)
				{
					_forceCheckpointEvent.Reset();
				}

				_transactions.TryRemove(info.Snapshot.TransactionId, out _);
				if (_transactions.IsEmpty)
				{
					_checkpoint();
				}
			}
		}

		public void Rollback(Snapshot snapshot)
		{
			lock (_sync)
			{
				if (_transactions.TryRemove(snapshot.TransactionId, out _))
				{
					if (_transactions.IsEmpty)
					{
						_checkpoint();
					}
				}		
			}
		}

		public void Restore()
		{
			if (_restored)
			{
				throw new BarbadosInternalErrorException();
			}

			Debug.Assert(_cache.Count == 0);
			Debug.Assert(_transactions.IsEmpty);
			Debug.Assert(_onDiskCommittedPageBuffers.IsEmpty);

			_restored = true;
			_currentWalOffset = Constants.WalHeaderLength;

			Span<byte> rb = stackalloc byte[Constants.WalRecordLength];

			var transactionPages = new Dictionary<ObjectId, Dictionary<PageHandle, long>>();
			var brokenChecksumRecordOffsets = new HashSet<long>();
			while (_currentWalOffset < _wal.Length)
			{
				var r = _wal.Read(_currentWalOffset, rb);
				if (r < Constants.WalRecordLength)
				{
					break;
				}

				var record = new WalRecord(rb);
				if (!record.VerifyChecksum())
				{
					throw new BarbadosException(
						BarbadosExceptionCode.ChecksumVerificationFailed, "Corrupted record found during restore"
					);
				}

				else
				{
					if (record.Marker == WalRecordTypeMarker.Page)
					{
						if (!transactionPages.TryGetValue(record.TransactionId, out var pages))
						{
							pages = [];
							transactionPages.Add(record.TransactionId, pages);
						}

						var buffer = new PageBuffer();
						r = _wal.Read(_currentWalOffset + Constants.WalRecordLength, buffer.AsSpan());
						if (r < Constants.PageLength)
						{
							break;
						}

						if (!AbstractPage.VerifyChecksum(buffer))
						{
							brokenChecksumRecordOffsets.Add(_currentWalOffset);
						}

						var handle = AbstractPage.GetPageHandle(buffer);
						if (!pages.TryAdd(handle, _currentWalOffset))
						{
							pages[handle] = _currentWalOffset;
						}

						_currentWalOffset += Constants.PageLength;
					}

					else
					if (record.Marker == WalRecordTypeMarker.Commit)
					{
						if (transactionPages.TryGetValue(record.TransactionId, out var pages))
						{
							foreach (var (handle, offset) in pages)
							{
								if (brokenChecksumRecordOffsets.Contains(offset))
								{
									throw new BarbadosException(
										BarbadosExceptionCode.ChecksumVerificationFailed, "Corrupted record found in a commited transaction"
									);
								}

								else
								{
									_onDiskCommittedPageBuffers.AddOrUpdate(handle, offset, (_, _) => offset);
								}
							}
						}
					}

					else
					{
						throw new BarbadosInternalErrorException();
					}
				}

				_currentWalOffset += Constants.WalRecordLength;
			}

			_checkpoint();
		}

		private void _incrementCommitId()
		{
			if (_latestCommitId.Value == ObjectId.MaxValue.Value)
			{
				throw new BarbadosException(BarbadosExceptionCode.MaxWalCommitNumberReached);
			}

			_latestCommitId = new ObjectId(_latestCommitId.Value + 1);
		}

		private void _flushAll()
		{
			foreach (var info in _transactions.Values)
			{
				_flush(info, addToCommittedList: false);
			}
		}

		private void _flush(WalTransactionInfo info, bool addToCommittedList)
		{
			Span<byte> rb = stackalloc byte[Constants.WalRecordLength];
			foreach (var (handle, buffer) in info.PageBuffers)
			{
				var start = _currentWalOffset;
				var record = new WalRecord(WalRecordTypeMarker.Page, info.Snapshot.TransactionId);

				// Includes checksum
				record.WriteTo(rb);

				_wal.Write(_currentWalOffset, rb);
				_currentWalOffset += Constants.WalRecordLength;

				// All buffers must have checksums at this point
				_wal.Write(_currentWalOffset, buffer.AsSpan());
				_currentWalOffset += Constants.PageLength;

				if (!info.OnDiskPageBuffers.TryAdd(handle, start))
				{
					info.OnDiskPageBuffers[handle] = start;
				}

				if (addToCommittedList)
				{
					_onDiskCommittedPageBuffers.AddOrUpdate(handle, start, (_, _) => start);
				}
			}

			_walPageCount += info.PageBuffers.Count;
			_bufferedPageCount -= info.PageBuffers.Count;
			info.PageBuffers.Clear();
		}

		private void _checkpoint()
		{
			Span<byte> rb = stackalloc byte[Constants.WalRecordLength];
			foreach (var (handle, offset) in _onDiskCommittedPageBuffers)
			{
				var pb = _readPageBufferFromWal(offset);
				_db.Write(handle.GetAddress(), pb.AsSpan());
			}

			_db.Persist();
			_wal.Truncate(Constants.WalHeaderLength);
			_wal.Persist();
			_currentWalOffset = Constants.WalHeaderLength;
			_walPageCount = 0;
			_bufferedPageCount -= _allocatorTransactionInfo.PageBuffers.Count;
			_allocatorSnapshot = _takeSnapshot(_allocatorTransactionId);
			_allocatorTransactionInfo = new WalTransactionInfo(_allocatorSnapshot);
			_onDiskCommittedPageBuffers.Clear();
			_forceCheckpointEvent.Set();
		}

		private Snapshot _takeSnapshot(ObjectId transactionId)
		{
			return new Snapshot()
			{
				TransactionId = transactionId,
				LatestCommitId = _latestCommitId
			};
		}

		private WalTransactionInfo _getTransactionInfo(Snapshot snapshot)
		{
			if (snapshot.TransactionId.Value == -1)
			{
				return _allocatorTransactionInfo;
			}

			if (!_transactions.TryGetValue(snapshot.TransactionId, out var info))
			{
				throw new BarbadosException(BarbadosExceptionCode.TransactionDoesNotExist);
			}

			return info;
		}
	}
}
