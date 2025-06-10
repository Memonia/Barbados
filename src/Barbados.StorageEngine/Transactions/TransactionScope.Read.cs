using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Transactions
{
	internal partial class TransactionScope
	{
		public T Load<T>(PageHandle handle) where T : AbstractPage
		{
			return _wal.LoadPin<T>(Snapshot, handle);
		}

		public bool IsPageType(PageHandle handle, PageMarker marker)
		{
			var buffer = _wal.LoadPin(Snapshot, handle);
			return AbstractPage.GetPageMarker(buffer) == marker;
		}
	}
}
