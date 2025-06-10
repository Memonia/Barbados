using System;

using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Collections.Extensions;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Wal;
using Barbados.StorageEngine.Storage.Wal.Pages;
using Barbados.StorageEngine.Transactions;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine
{
	internal sealed class DatabaseFacade : IDatabaseFacade
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

		public DatabaseFacade(WalBuffer wal, TimeSpan transactionAcquireLockTimeout)
		{
			_lockManager = new();
			_txManager = new(transactionAcquireLockTimeout, wal, _lockManager);

			// Initialise meta collection
			_lockManager.CreateLock(MetaCollectionFacade.MetaCollectionId);
			var meta = _createMetaFacade();

			// Create locks for existing collections
			using var cursor = meta.Find(FindOptions.All);
			foreach (var document in cursor)
			{
				_lockManager.CreateLock(document.GetObjectId());
			}

			var ics = new IndexControllerService();
			Collections = new(_lockManager, _txManager, meta, ics);
			Indexes = new(_txManager, meta, Collections, ics);
			Monitor = new(meta);
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
				MetaCollectionFacade.MetaCollectionId, TransactionMode.Read
			);

			var root = tx.Load<RootPage>(PageHandle.Root);
			var mcinfo = new CollectionInfo()
			{
				BTreeInfo = new BTreeInfo(root.MetaCollectionPageHandle),
				CollectionId = MetaCollectionFacade.MetaCollectionId,
				AutomaticIdGeneratorMode = AutomaticIdGeneratorMode.BetterSpaceUtilisation
			};

			var niinfo = new IndexInfo(
				new BTreeInfo(root.MetaCollectionNameIndexRootPageHandle),
				BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField
			);

			return new MetaCollectionFacade(mcinfo, niinfo, TransactionManager);
		}
	}
}
