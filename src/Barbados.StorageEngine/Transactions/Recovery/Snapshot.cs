namespace Barbados.StorageEngine.Transactions.Recovery
{
	internal sealed class Snapshot
	{
		public required ObjectId TransactionId { get; init; }
		public required ObjectId LatestCommitId { get; init; }
	}
}
