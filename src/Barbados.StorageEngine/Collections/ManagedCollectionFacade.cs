using System.Collections.Generic;

using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class ManagedCollectionFacade : BaseBarbadosCollectionFacade
	{
		public override BarbadosDbObjectName Name => _collectionControllerService.GetCollectionName(Id);

		private readonly IndexControllerService _indexControllerService;
		private readonly CollectionControllerService _collectionControllerService;

		public ManagedCollectionFacade(
			ObjectId id,
			TransactionManager transactionManager,
			BTreeClusteredIndexFacade clusteredIndexFacade,
			IndexControllerService indexControllerService,
			CollectionControllerService collectionControllerService
		) : base(id, transactionManager, clusteredIndexFacade)
		{
			_indexControllerService = indexControllerService;
			_collectionControllerService = collectionControllerService;
		}

		public override bool TryGetBTreeIndex(string field, out IReadOnlyBTreeIndex index)
		{
			if (_indexControllerService.TryGetFacade(Id, field, out var facade))
			{
				index = facade;
				return true;
			}

			index = default!;
			return false;
		}

		protected override IEnumerable<BTreeIndexFacade> EnumerateIndexes()
		{
			return _indexControllerService.EnumerateBTreeIndexFacades(ClusteredIndexFacade);
		}
	}
}
