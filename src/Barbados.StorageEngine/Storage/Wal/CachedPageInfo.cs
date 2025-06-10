using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Storage.Wal
{
	internal sealed class CachedPageInfo
	{
		public required ObjectId LatestCommitId { get; init; }
		public required PageBuffer Buffer { get; init; }
	}
}
