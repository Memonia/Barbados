using System.Collections.Generic;

using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Storage.Wal
{
	internal sealed class WalTransactionInfo
	{
		public Snapshot Snapshot { get; }
		public HashSet<PageHandle> Allocated { get; }
		public HashSet<PageHandle> Deallocated { get; }
		public Dictionary<PageHandle, PageBuffer> PageBuffers { get; }
		public Dictionary<PageHandle, long> OnDiskPageBuffers { get; }

		public WalTransactionInfo(Snapshot snapshot)
		{
			Snapshot = snapshot;
			Allocated = [];
			Deallocated = [];
			PageBuffers = [];
			OnDiskPageBuffers = [];
		}
	}
}
