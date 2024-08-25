using System.Collections.Generic;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal partial class MetaCollectionFacade
	{
		public static int NameIndexKeyMaxLength { get; } = 64;
		public static ObjectId MetaCollectionId { get; } = new(-1);

		public static IEnumerable<BarbadosDocument> EnumerateIndexDocuments(BarbadosDocument document)
		{
			if (document.TryGetDocumentArray(CommonIdentifiers.MetaCollection.IndexArrayField, out var indexesArray))
			{
				return indexesArray;
			}

			return [];
		}

		public static BTreeIndexFacade CreateBTreeIndexFacade(
			BarbadosDocument document,
			TransactionManager transactionManager,
			BTreeClusteredIndexFacade clusteredIndexFacade
		)
		{
			var rawHandle = document.GetInt64(
				CommonIdentifiers.MetaCollection.IndexDocumentPageHandleField
			);
			var indexField = document.GetString(
				CommonIdentifiers.MetaCollection.IndexDocumentIndexedFieldField
			); 
			var keyMaxLength = document.GetInt32(
				CommonIdentifiers.MetaCollection.IndexDocumentKeyMaxLengthField
			);

			var info = new BTreeIndexInfo()
			{
				CollectionId = clusteredIndexFacade.Info.CollectionId,
				RootHandle = new(rawHandle),
				IndexField = indexField,
				KeyMaxLength = keyMaxLength
			};

			return new BTreeIndexFacade(transactionManager, clusteredIndexFacade, info);
		}

		public static BarbadosCollectionFacade CreateBarbadosCollectionFacade(
			BarbadosDocument document,
			TransactionManager transactionManager,
			IndexControllerService indexControllerService,
			CollectionControllerService collectionControllerService
		)
		{
			var collectionPageHandleRaw = document.GetInt64(
				CommonIdentifiers.MetaCollection.AbsCollectionDocumentPageHandleField
			);

			return new BarbadosCollectionFacade(
				document.Id,
				transactionManager,
				new BTreeClusteredIndexFacade(document.Id, new(collectionPageHandleRaw)),
				indexControllerService,
				collectionControllerService
			);
		}
	}
}
