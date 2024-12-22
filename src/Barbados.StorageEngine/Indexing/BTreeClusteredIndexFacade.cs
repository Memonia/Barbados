using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed class BTreeClusteredIndexFacade : AbstractBTreeIndexFacade
	{
		public BTreeClusteredIndexFacade(ObjectId collectionId, PageHandle handle) :
			base(
				new()
				{
					CollectionId = collectionId,
					RootHandle = handle,
					IndexField = BarbadosDocumentKeys.DocumentId,
					KeyMaxLength = Constants.ObjectIdLength
				}
			)
		{

		}

		public BTreeClusteredIndexTransactionProxy GetProxy(TransactionScope transaction)
		{
			return new(transaction, Info);
		}
	}
}
