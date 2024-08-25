using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Transactions.Recovery
{
	internal sealed class CachedPageInfo
	{
		public required ObjectId LatestCommitId { get; init; }
		public required PageBuffer Buffer { get; init; }
	}
}
