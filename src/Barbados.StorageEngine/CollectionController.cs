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

		public CollectionController(
			LockManager lockManager,
			TransactionManager transactionManager,
			MetaCollectionFacade metaCollectionFacade,
			IndexControllerService indexControllerService
		)
		{
			_lockManager = lockManager;
			_txManager = transactionManager;
			_metaFacade = metaCollectionFacade;
			_indexControllerService = indexControllerService;
		}

		public bool TryGet(ObjectId collectionId, out ManagedCollectionFacade facade)
		{
			return TryGet(collectionId, out facade, out _);
		}
	
		public bool TryGet(ObjectId collectionId, out ManagedCollectionFacade facade, out BarbadosDocument document)
		{
			using var tx = _txManager.CreateTransaction(TransactionMode.Read)
				.IncludeLock(_metaFacade.Id, LockMode.Read)
				.BeginTransaction();

			if (!_metaFacade.TryGetCollectionDocument(collectionId, out document))
			{
				facade = default!;
				return false;
			}

			facade = MetaCollectionFacade.CreateBarbadosCollectionFacade(
				document, _txManager, _indexControllerService, new CollectionMetadataService(_metaFacade, collectionId)
			);

			return true;
		}

		public bool TryRename(ObjectId collectionId, string replacement)
		{
			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.BeginTransaction();

			if (_metaFacade.TryGetCollectionId(replacement, out _))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionAlreadyExists(replacement);
			}

			if (!_metaFacade.TryGetCollectionDocument(collectionId, out var document))
			{
				return false;
			}

			_metaFacade.Rename(document, replacement);
			_txManager.CommitTransaction(tx);
			return true;
		}

		public bool TryDelete(ObjectId collectionId)
		{
			if (!TryGet(collectionId, out var facade, out var document))
			{
				return false;
			}

			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.IncludeLock(facade.Id, LockMode.Write)
				.BeginTransaction();

			if (!_metaFacade.TryRemove(document))
			{
				throw new BarbadosInternalErrorException();
			}

			_lockManager.RemoveLock(collectionId, out _);
			_indexControllerService.TryRemove(collectionId);

			facade.Deallocate();
			_txManager.CommitTransaction(tx);
			return true;
		}
	
		public bool TryGet(string collection, out ManagedCollectionFacade facade)
		{
			if (!_metaFacade.TryGetCollectionId(collection, out var id))
			{
				facade = default!;
				return false;
			}

			return TryGet(id, out facade);
		}
		
		public bool TryCreate(string collection, CreateCollectionOptions options)
		{
			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.BeginTransaction();

			if (_metaFacade.TryGetCollectionId(collection, out _))
			{
				return false;
			}

			var collectionId = _metaFacade.Create(collection, options);
			_lockManager.CreateLock(collectionId);
			_txManager.CommitTransaction(tx);
			return true;
		}

		public bool TryRename(string collection, string replacement)
		{
			if (!_metaFacade.TryGetCollectionId(collection, out var id))
			{
				return false;
			}

			return TryRename(id, replacement);
		}

		public bool TryDelete(string collection)
		{
			if (!_metaFacade.TryGetCollectionId(collection, out var id))
			{
				return false;
			}

			return TryDelete(id);
		}
	}
}
