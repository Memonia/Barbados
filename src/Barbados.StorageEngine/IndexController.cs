using System.Collections.Generic;
using System.Linq;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Transactions;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine
{
	internal sealed partial class IndexController : IIndexController
	{
		private readonly TransactionManager _txManager;
		private readonly MetaCollectionFacade _metaFacade;
		private readonly CollectionController _collectionController;
		private readonly IndexControllerService _indexControllerService;

		public IndexController(
			TransactionManager transactionManager,
			MetaCollectionFacade metaCollectionFacade,
			CollectionController collectionController,
			IndexControllerService indexControllerService
		)
		{
			_txManager = transactionManager;
			_metaFacade = metaCollectionFacade;
			_collectionController = collectionController;
			_indexControllerService = indexControllerService;
		}

		public void LoadIndexes()
		{
			var indexDocuments = new Dictionary<ObjectId, List<BarbadosDocument>>();
			foreach (var document in _metaFacade.GetCursor())
			{
				indexDocuments.Add(
					document.Id, MetaCollectionFacade.EnumerateIndexDocuments(document).ToList()
				);
			}

			foreach (var (id, docs) in indexDocuments)
			{
				foreach (var doc in docs)
				{
					if (!_collectionController.TryGet(id, out var collection))
					{
						throw new BarbadosInternalErrorException();
					}

					_indexControllerService.AddFacade(
						MetaCollectionFacade.CreateBTreeIndexFacade(
							doc, _txManager, collection.ClusteredIndexFacade
						)
					);
				}
			}
		}

		public bool TryGet(ObjectId collectionId, string field, out BTreeIndexFacade facade)
		{
			return _indexControllerService.TryGetFacade(collectionId, field, out facade);
		}

		public bool TryCreate(ObjectId collectionId, string field, int maxKeyLength, bool useDefault)
		{
			if (!_collectionController.TryGet(collectionId, out var collection, out var document))
			{
				return false;
			}

			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(collectionId, LockMode.Write)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.BeginTransaction();

			var idoc = useDefault
				? _metaFacade.CreateIndex(document, field)
				: _metaFacade.CreateIndex(document, field, maxKeyLength);

			var facade = MetaCollectionFacade.CreateBTreeIndexFacade(idoc, _txManager, collection.ClusteredIndexFacade);
			collection.BTreeIndexBuild(facade);

			_indexControllerService.AddFacade(facade);
			_txManager.CommitTransaction(tx);
			return true;
		}

		public bool TryDelete(ObjectId collectionId, string field)
		{
			if (!_indexControllerService.TryGetFacade(collectionId, field, out var facade))
			{
				return false;
			}

			if (!_collectionController.TryGet(collectionId, out _, out var document))
			{
				return false;
			}

			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(collectionId, LockMode.Write)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.BeginTransaction();

			_indexControllerService.TryRemoveFacade(collectionId, field, out _);
			_metaFacade.RemoveIndex(document, field);
			facade.Deallocate(tx);
			_txManager.CommitTransaction(tx);
			return true;
		}

		public bool TryGet(string collectionName, string field, out BTreeIndexFacade facade)
		{
			if (!_metaFacade.Find(collectionName, out var id))
			{
				facade = default!;
				return false;
			}

			return TryGet(id, field, out facade);
		}

		public bool TryCreate(string collectionName, string field, int maxKeyLength, bool useDefault)
		{
			if (!_metaFacade.Find(collectionName, out var id))
			{
				return false;
			}

			return TryCreate(id, field, maxKeyLength, useDefault);
		}

		public bool TryDelete(string collectionName, string field)
		{
			if (!_metaFacade.Find(collectionName, out var id))
			{
				return false;
			}

			return TryDelete(id, field);
		}
	}
}
