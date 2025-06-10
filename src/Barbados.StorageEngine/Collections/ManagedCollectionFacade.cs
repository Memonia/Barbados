using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class ManagedCollectionFacade : BaseBarbadosCollectionFacade, IBarbadosCollection
	{
		public override BarbadosDbObjectName Name => _metadataService.GetName();

		private readonly IndexControllerService _indexControllerService;
		private readonly CollectionMetadataService _metadataService;

		public ManagedCollectionFacade(
			CollectionInfo info,
			TransactionManager transactionManager,
			CollectionMetadataService metadataService,
			IndexControllerService indexControllerService
		) : base(info, transactionManager)
		{
			_indexControllerService = indexControllerService;
			_metadataService = metadataService;
		}

		public bool IndexExists(BarbadosKey field)
		{
			return _indexControllerService.TryGet(Id, field, out _);
		}

		protected override IndexInfo GetIndexInfo(BarbadosKey field)
		{
			if (!_indexControllerService.TryGet(Id, field, out var info))
			{
				BarbadosCollectionExceptionHelpers.ThrowIndexDoesNotExist(Id, field.ToString());
			}

			return info;
		}

		protected override IEnumerable<IndexInfo> EnumerateIndexes()
		{
			return _indexControllerService.EnumerateIndexes(Id);
		}
	}
}
