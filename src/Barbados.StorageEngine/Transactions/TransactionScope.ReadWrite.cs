using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Transactions
{
	internal partial class TransactionScope
	{
		public void Save(AbstractPage page)
		{
			_throwModeMismatch(TransactionMode.ReadWrite);
			_wal.Save(Snapshot, page);
		}

		public PageHandle AllocateHandle()
		{
			_throwModeMismatch(TransactionMode.ReadWrite);
			return _wal.Allocate(Snapshot);
		}

		public void Deallocate(PageHandle handle)
		{
			_throwModeMismatch(TransactionMode.ReadWrite);
			_wal.Deallocate(Snapshot, handle);
		}
	}
}
