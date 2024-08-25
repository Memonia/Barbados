using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed partial class BTreeIndexTransactionProxy : AbstractBTreeIndexTransactionProxy<BTreeLeafPage>
	{
		public BTreeIndexTransactionProxy(TransactionScope Transaction, BTreeIndexInfo info) :
			base(Transaction, info)
		{

		}
	}
}
