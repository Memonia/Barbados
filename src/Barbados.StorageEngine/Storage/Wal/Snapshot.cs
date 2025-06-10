namespace Barbados.StorageEngine.Storage.Wal
{
	internal sealed class Snapshot
	{
		public required ObjectId TransactionId { get; init; }
		public required ObjectId LatestCommitId { get; init; }
	}
}
