using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine
{
	internal sealed partial class DatabaseFacade : IDatabaseFacade
	{
		IDatabaseMonitor IDatabaseFacade.Monitor => Monitor;
		IIndexController IDatabaseFacade.Indexes => Indexes;
		ICollectionController IDatabaseFacade.Collections => Collections;

		public TransactionManager TransactionManager => _txManager;

		public DatabaseMonitor Monitor { get; }
		public IndexController Indexes { get; }
		public CollectionController Collections { get; }

		private readonly LockManager _lockManager;
		private readonly TransactionManager _txManager;

		public DatabaseFacade(LockManager lockManager, TransactionManager transactionManager)
		{
			_lockManager = lockManager;
			_txManager = transactionManager;

			// Initialise meta collection
			_lockManager.CreateLock(MetaCollectionFacade.MetaCollectionId);
			var meta = _createMetaFacade();

			// Create locks for existing collections
			foreach (var document in meta.GetCursor(ValueSelector.SelectNone))
			{
				_lockManager.CreateLock(document.Id);
			}

			var ics = new IndexControllerService();
			var ccs = new CollectionControllerService(meta);
			Collections = new(_lockManager, _txManager, meta, ics, ccs);
			Indexes = new(_txManager, meta, Collections, ics);
			Monitor = new(meta);

			Indexes.LoadIndexes();
		}

		public ITransactionBuilder CreateTransaction(TransactionMode mode)
		{
			return _txManager.CreateTransaction(mode);
		}

		public void CommitTransaction()
		{
			_txManager.CommitCurrentTransaction();
		}

		public void RollbackTransaction()
		{
			_txManager.RollbackCurrentTransaction();
		}

		private MetaCollectionFacade _createMetaFacade()
		{
			using var tx = _txManager.GetAutomaticTransaction(
				MetaCollectionFacade.MetaCollectionId, TransactionMode.ReadWrite
			);

			var root = tx.Load<RootPage>(PageHandle.Root);
			var clusteredIndexFacade = new BTreeClusteredIndexFacade(
				MetaCollectionFacade.MetaCollectionId, root.MetaCollectionPageHandle
			);

			var nameIndexFacade = new BTreeIndexFacade(
				_txManager, clusteredIndexFacade,
				new()
				{
					CollectionId = MetaCollectionFacade.MetaCollectionId,
					RootHandle = root.MetaCollectionNameIndexRootPageHandle,
					IndexField = CommonIdentifiers.MetaCollection.AbsCollectionDocumentNameField,
					KeyMaxLength = MetaCollectionFacade.NameIndexKeyMaxLength
				}
			);

			return new MetaCollectionFacade(_txManager, clusteredIndexFacade, nameIndexFacade);
		}
	}
}
