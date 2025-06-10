using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Collections.Extensions;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal partial class MetaCollectionFacade
	{
		public static ObjectId MetaCollectionId { get; } = new(-1);

		public static IEnumerable<BarbadosDocument> EnumerateIndexDocuments(BarbadosDocument document)
		{
			if (document.TryGetDocumentArray(BarbadosDocumentKeys.MetaCollection.IndexArrayField, out var indexesArray))
			{
				return indexesArray;
			}

			return [];
		}

		public static IndexInfo CreateIndexInfo(BarbadosDocument indexDocument)
		{
			var rawHandle = indexDocument.GetInt64(
				BarbadosDocumentKeys.MetaCollection.IndexDocumentPageHandleField
			);

			var field = indexDocument.GetString(
				BarbadosDocumentKeys.MetaCollection.IndexDocumentIndexedFieldField
			);

			return new IndexInfo(
				new BTreeInfo(new PageHandle(rawHandle)), field
			);
		}

		public static ManagedCollectionFacade CreateBarbadosCollectionFacade(
			BarbadosDocument document,
			TransactionManager transactionManager,
			IndexControllerService indexControllerService,
			CollectionMetadataService metadataService
		)
		{
			var collectionPageHandleRaw = document.GetInt64(
				BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentPageHandleField
			);

			var idGenMode = document.GetInt32(
				BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentIdGenModeField
			);

			var info = new CollectionInfo()
			{
				BTreeInfo = new BTreeInfo(new PageHandle(collectionPageHandleRaw)),
				CollectionId = document.GetObjectId(),
				AutomaticIdGeneratorMode = (AutomaticIdGeneratorMode)idGenMode
			};

			return new ManagedCollectionFacade(
				info, transactionManager, metadataService, indexControllerService
			);
		}
	}
}
