using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine
{
	internal sealed partial class CollectionController : ICollectionController
	{
		private readonly LockManager _lockManager;
		private readonly TransactionManager _txManager;
		private readonly MetaCollectionFacade _metaFacade;
		private readonly IndexControllerService _indexControllerService;
		private readonly CollectionControllerService _collectionControllerService;

		public CollectionController(
			LockManager lockManager,
			TransactionManager transactionManager,
			MetaCollectionFacade metaCollectionFacade,
			IndexControllerService indexControllerService,
			CollectionControllerService collectionControllerService
		)
		{
			_lockManager = lockManager;
			_txManager = transactionManager;
			_metaFacade = metaCollectionFacade;
			_indexControllerService = indexControllerService;
			_collectionControllerService = collectionControllerService;
		}

		public bool TryGet(ObjectId collectionId, out BarbadosCollectionFacade facade)
		{
			return TryGet(collectionId, out facade, out _);
		}
	
		public bool TryGet(ObjectId collectionId, out BarbadosCollectionFacade facade, out BarbadosDocument document)
		{
			using var tx = _txManager.CreateTransaction(TransactionMode.Read)
				.IncludeLock(_metaFacade.Id, LockMode.Read)
				.BeginTransaction();

			if (!_metaFacade.TryRead(collectionId, BarbadosKeySelector.SelectAll, out document))
			{
				facade = default!;
				return false;
			}

			facade = MetaCollectionFacade.CreateBarbadosCollectionFacade(
				document, _txManager, _indexControllerService, _collectionControllerService
			);

			return true;
		}

		public bool TryRename(ObjectId collectionId, string replacement)
		{
			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.BeginTransaction();

			if (_metaFacade.Find(replacement, out _))
			{
				BarbadosCollectionException.ThrowCollectionAlreadyExists(replacement);
			}

			if (!_metaFacade.TryRead(collectionId, BarbadosKeySelector.SelectAll, out var document))
			{
				return false;
			}

			_metaFacade.Rename(document, replacement);
			_txManager.CommitTransaction(tx);
			return true;
		}

		public bool TryDelete(ObjectId collectionId)
		{
			if (!TryGet(collectionId, out var facade, out _))
			{
				return false;
			}

			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.IncludeLock(facade.Id, LockMode.Write)
				.BeginTransaction();

			_lockManager.RemoveLock(collectionId, out _);
			_indexControllerService.TryRemoveFacades(collectionId);
			if (!_metaFacade.TryRemove(collectionId))
			{
				throw new BarbadosInternalErrorException();
			}

			facade.Deallocate();
			_txManager.CommitTransaction(tx);
			return true;
		}
	
		public bool TryGet(string collection, out BarbadosCollectionFacade facade)
		{
			if (!_metaFacade.Find(collection, out var id))
			{
				facade = default!;
				return false;
			}

			return TryGet(id, out facade);
		}
		
		public bool TryCreate(string collection)
		{
			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.BeginTransaction();

			if (_metaFacade.Find(collection, out _))
			{
				return false;
			}

			var collectionId = _metaFacade.Create(collection);
			_lockManager.CreateLock(collectionId);
			_txManager.CommitTransaction(tx);
			return true;
		}

		public bool TryRename(string collection, string replacement)
		{
			if (!_metaFacade.Find(collection, out var id))
			{
				return false;
			}

			return TryRename(id, replacement);
		}

		public bool TryDelete(string collection)
		{
			if (!_metaFacade.Find(collection, out var id))
			{
				return false;
			}

			return TryDelete(id);
		}
	}
}
