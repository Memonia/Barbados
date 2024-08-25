using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine
{
	public interface IDatabaseFacade
	{
		IDatabaseMonitor Monitor { get; }
		IIndexController Indexes { get; }
		ICollectionController Collections { get; }

		ITransactionBuilder CreateTransaction(TransactionMode mode);
		void CommitTransaction();
		void RollbackTransaction();
	}
}
