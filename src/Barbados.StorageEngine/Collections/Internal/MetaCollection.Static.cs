using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Paging;

namespace Barbados.StorageEngine.Collections.Internal
{
	internal partial class MetaCollection
	{
		public static int NameIndexKeyMaxLength { get; } = 64;

		public static IEnumerable<BarbadosDocument> GetIndexDocuments(BarbadosDocument document)
		{
			if (document.TryGetDocumentArray(BarbadosIdentifiers.MetaCollection.IndexArrayField, out var indexesArray))
			{
				return indexesArray;
			}

			return [];
		}

		public static BTreeIndex CreateIndexInstance(
			BarbadosDocument document,
			PagePool pool,
			LockAutomatic collectionLock,
			BTreeClusteredIndex clusteredIndex
		)
		{
			var a = document.TryGetInt64(
				BarbadosIdentifiers.MetaCollection.IndexDocumentPageHandleField,
				out var rawHandle
			);

			var b = document.TryGetInt32(
				BarbadosIdentifiers.MetaCollection.IndexDocumentKeyMaxLengthField,
				out var keyMaxLength
			);

			var c = document.TryGetString(
				BarbadosIdentifiers.MetaCollection.IndexDocumentIndexedFieldField,
				out var indexedField
			);

			Debug.Assert(a);
			Debug.Assert(b);
			Debug.Assert(c);

			var info = new BTreeIndexInfo()
			{
				IndexedField = indexedField,
				KeyMaxLength = keyMaxLength,
				RootPageHandle = new(rawHandle)
			};

			return new BTreeIndex(info, collectionLock, clusteredIndex, pool);
		}

		public static BarbadosCollection CreateCollectionInstance(
			BarbadosDocument document,
			PagePool pool,
			LockAutomatic @lock
		)
		{
			var a = document.TryGetString(
				BarbadosIdentifiers.MetaCollection.CollectionDocumentNameFieldAbsolute,
				out var collection
			);

			var b = document.TryGetInt64(
				BarbadosIdentifiers.MetaCollection.CollectionDocumentPageHandleFieldAbsolute,
				out var collectionPageHandleRaw
			);

			Debug.Assert(a);
			Debug.Assert(b);

			var clusteredIndex = new BTreeClusteredIndex(pool, new(collectionPageHandleRaw));
			return new BarbadosCollection(collection, pool, @lock, clusteredIndex);
		}
	}
}
