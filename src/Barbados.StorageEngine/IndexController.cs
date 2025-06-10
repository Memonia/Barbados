using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Collections.Extensions;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Exceptions;
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

			_loadIndexInfo();
		}

		public bool TryGet(ObjectId collectionId, BarbadosKey field, out IndexInfo info)
		{
			return _indexControllerService.TryGet(collectionId, field, out info);
		}

		public bool TryCreate(ObjectId collectionId, BarbadosKey field)
		{
			if (!_collectionController.TryGet(collectionId, out var collection, out var document))
			{
				return false;
			}

			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(collectionId, LockMode.Write)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.BeginTransaction();

			var idoc = _metaFacade.CreateIndex(document, field);
			var info = MetaCollectionFacade.CreateIndexInfo(idoc);
			collection.IndexBuild(info);

			_indexControllerService.Add(collectionId, info);
			_txManager.CommitTransaction(tx);
			return true;
		}

		public bool TryDelete(ObjectId collectionId, string field)
		{
			if (!_indexControllerService.TryGet(collectionId, field, out var info))
			{
				return false;
			}

			if (!_collectionController.TryGet(collectionId, out var facade, out var document))
			{
				return false;
			}

			using var tx = _txManager.CreateTransaction(TransactionMode.ReadWrite)
				.IncludeLock(collectionId, LockMode.Write)
				.IncludeLock(_metaFacade.Id, LockMode.Write)
				.BeginTransaction();

			facade.IndexDeallocate(info);
			_metaFacade.RemoveIndex(document, field);

			if (!_indexControllerService.TryRemove(collectionId, field, out _))
			{
				throw new BarbadosInternalErrorException();
			}

			_txManager.CommitTransaction(tx);
			return true;
		}

		public bool TryGet(string collectionName, string field, out IndexInfo info)
		{
			if (!_metaFacade.TryGetCollectionId(collectionName, out var id))
			{
				info = default!;
				return false;
			}

			return TryGet(id, field, out info);
		}

		public bool TryCreate(string collectionName, string field)
		{
			if (!_metaFacade.TryGetCollectionId(collectionName, out var id))
			{
				return false;
			}

			return TryCreate(id, field);
		}

		public bool TryDelete(string collectionName, string field)
		{
			if (!_metaFacade.TryGetCollectionId(collectionName, out var id))
			{
				return false;
			}

			return TryDelete(id, field);
		}

		private void _loadIndexInfo()
		{
			var indexDocuments = new Dictionary<ObjectId, List<BarbadosDocument>>();
			using var cursor = _metaFacade.Find(FindOptions.All);
			foreach (var document in cursor)
			{
				indexDocuments.Add(
					document.GetObjectId(), [.. MetaCollectionFacade.EnumerateIndexDocuments(document)]
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

					_indexControllerService.Add(id, MetaCollectionFacade.CreateIndexInfo(doc));
				}
			}
		}
	}
}
