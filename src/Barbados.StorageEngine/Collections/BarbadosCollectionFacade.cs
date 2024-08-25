using System.Collections.Generic;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class BarbadosCollectionFacade : AbstractCollectionFacade, IBarbadosCollection
	{
		public BarbadosIdentifier Name => _collectionControllerService.GetCollectionName(Id);

		private readonly IndexControllerService _indexControllerService;
		private readonly CollectionControllerService _collectionControllerService;

		public BarbadosCollectionFacade(
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

		public bool TryGetBTreeIndex(string field, out IReadOnlyBTreeIndex index)
		{
			if (_indexControllerService.TryGetFacade(Id, field, out var facade))
			{
				index = facade;
				return true;
			}

			index = default!;
			return false;
		}

		public bool TryRead(ObjectId id, out BarbadosDocument document)
		{
			return TryRead(id, ValueSelector.SelectAll, out document);
		}

		public BarbadosDocument Read(ObjectId id)
		{
			return Read(id, ValueSelector.SelectAll);
		}

		public BarbadosDocument Read(ObjectId id, ValueSelector selector)
		{
			if (!TryRead(id, selector, out var document))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DocumentNotFound, $"Document with id {id} not found"
				);
			}

			return document;
		}

		public void Update(ObjectId id, BarbadosDocument document)
		{
			if (!TryUpdate(id, document))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DocumentNotFound, $"Document with id {id} not found"
				);
			}
		}

		public void Remove(ObjectId id)
		{
			if (!TryRemove(id))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DocumentNotFound, $"Document with id {id} not found"
				);
			}
		}

		ICursor<BarbadosDocument> IReadOnlyBarbadosCollection.GetCursor()
		{
			return GetCursor();
		}

		ICursor<BarbadosDocument> IReadOnlyBarbadosCollection.GetCursor(ValueSelector selector)
		{
			return GetCursor(selector);
		}

		protected override IEnumerable<BTreeIndexFacade> EnumerateIndexes()
		{
			return _indexControllerService.EnumerateBTreeIndexFacades(ClusteredIndexFacade);
		}
	}
}
