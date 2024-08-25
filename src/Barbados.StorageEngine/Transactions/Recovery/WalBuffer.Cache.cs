using System;
using System.Diagnostics;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Transactions.Recovery
{
	internal partial class WalBuffer
	{
		private static T _copyBufferAndCreatePage<T>(PageBuffer buffer) where T : AbstractPage
		{
			var copy = new PageBuffer();
			buffer.AsSpan().CopyTo(copy.AsSpan());

			AbstractPage page;
			if (typeof(T) == typeof(BTreePage))
			{
				var marker = AbstractPage.GetPageMarker(copy);
				page = marker switch
				{
					PageMarker.BTreeNode => new BTreePage(copy),
					PageMarker.BTreeRoot => new BTreeRootPage(copy),
					PageMarker.Collection => new CollectionPage(copy),
					_ => throw new BarbadosException(BarbadosExceptionCode.InternalError),
				};
			}

			else
			{
				page = typeof(T) switch
				{
					Type rpage when rpage == typeof(RootPage) => new RootPage(copy),
					Type opage when opage == typeof(ObjectPage) => new ObjectPage(copy),
					Type apage when apage == typeof(AllocationPage) => new AllocationPage(copy),
					Type cpage when cpage == typeof(CollectionPage) => new CollectionPage(copy),
					Type brpage when brpage == typeof(BTreeRootPage) => new BTreeRootPage(copy),
					Type blpage when blpage == typeof(BTreeLeafPage) => new BTreeLeafPage(copy),
					Type opopage when opopage == typeof(ObjectPageOverflow) => new ObjectPageOverflow(copy),
					Type blopage when blopage == typeof(BTreeLeafPageOverflow) => new BTreeLeafPageOverflow(copy),
					_ => throw new BarbadosException(BarbadosExceptionCode.InternalError),
				};
			}

			return (T)page;
		}

		public T LoadPin<T>(Snapshot snapshot, PageHandle handle) where T : AbstractPage
		{
			var buffer = LoadPin(snapshot, handle);
			return _copyBufferAndCreatePage<T>(buffer);
		}

		public PageBuffer LoadPin(Snapshot snapshot, PageHandle handle)
		{
			var info = _getTransactionInfo(snapshot);

			// In WAL buffer?
			if (info.PageBuffers.TryGetValue(handle, out var buffer))
			{
				return buffer;
			}

			// In WAL file?
			if (info.OnDiskPageBuffers.TryGetValue(handle, out var offset))
			{
				return _readPageBufferFromWal(offset);
			}

			return _getMostRecentPageBufferVersion(snapshot, handle);
		}

		private PageBuffer _getMostRecentPageBufferVersion(Snapshot snapshot, PageHandle handle)
		{
			void cache(PageBuffer buffer)
			{
				lock (_sync)
				{
					_cache.TryCache(handle, new CachedPageInfo()
					{
						LatestCommitId = _latestCommitId,
						Buffer = buffer
					});
				}	
			}

			// In cache?
			if (_cache.TryGet(handle, out var cinfo))
			{
				Debug.Assert(cinfo.LatestCommitId.Value <= snapshot.LatestCommitId.Value);

				// Is cached page still valid?
				if (cinfo.LatestCommitId.Value == snapshot.LatestCommitId.Value)
				{
					return cinfo.Buffer;
				}
			}

			// Committed in WAL?
			if (_onDiskCommittedPageBuffers.TryGetValue(handle, out var offset))
			{
				var buf = _readPageBufferFromWal(offset);
				cache(buf);
				return buf;
			}

			// Fetch from the main database file
			var buffer = _readPageBufferFromDb(handle);
			cache(buffer);
			return buffer;
		}

		private PageBuffer _readPageBufferFromDb(PageHandle handle)
		{
			var buffer = new PageBuffer();
			var r = _db.Read(handle.GetAddress(), buffer.AsSpan());
			if (r < Constants.PageLength)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.UnexpectedEndOfFile, "Could not read a page from the database file"
				);
			}

			else
			if (r > Constants.PageLength)
			{
				throw new BarbadosInternalErrorException();
			}

			if (!AbstractPage.VerifyChecksum(buffer))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.ChecksumVerificationFailed, "Database page checksum verification failed"
				);
			}

			return buffer;
		}

		private PageBuffer _readPageBufferFromWal(long offset)
		{
			Span<byte> rb = stackalloc byte[Constants.WalRecordLength];
			var i = offset;
			var r = _wal.Read(i, rb);
			if (r < Constants.WalRecordLength)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.UnexpectedEndOfFile, "Could not read the WAL record"
				);
			}
			var record = new WalRecord(rb); 
			if (!record.VerifyChecksum())
			{
				throw new BarbadosException(
					BarbadosExceptionCode.ChecksumVerificationFailed, "WAL record checksum verification failed"
				);
			}

			if (record.Marker != WalRecordTypeMarker.Page)
			{
				throw new BarbadosInternalErrorException();
			}

			i += Constants.WalRecordLength;
			var pbuf = new PageBuffer();
			r = _wal.Read(i, pbuf.AsSpan());
			if (r < Constants.PageLength)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.UnexpectedEndOfFile, "Could not read the WAL record page"
				);
			}

			if (!AbstractPage.VerifyChecksum(pbuf))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.ChecksumVerificationFailed, "WAL record page checksum verification failed"
				);
			}

			return pbuf;
		}
	}
}
