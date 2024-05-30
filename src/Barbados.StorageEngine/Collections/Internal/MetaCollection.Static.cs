using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Paging;

namespace Barbados.StorageEngine.Collections.Internal
{
	internal partial class MetaCollection
	{
		public static string GetIndexName(BarbadosDocument document, string collection)
		{
			var r = document.TryGetString(
				BarbadosIdentifiers.MetaCollection.IndexDocumentIndexedFieldField, out var indexedField
			);
			Debug.Assert(r);

			return $"{collection}.{indexedField}";
		}

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
			string name,
			PagePool pool,
			LockAutomatic @lock,
			BTreeClusteredIndex clusteredIndex
		)
		{
			var a = document.TryGetInt64(
				BarbadosIdentifiers.MetaCollection.IndexDocumentPageHandleField, out var rawHandle
			);
			var b = document.TryGetInt32(
				BarbadosIdentifiers.MetaCollection.IndexDocumentKeyMaxLengthField, out var keyMaxLength
			);
			var c = document.TryGetString(
				BarbadosIdentifiers.MetaCollection.IndexDocumentIndexedFieldField, out var indexedField
			);
			Debug.Assert(a);
			Debug.Assert(b);
			Debug.Assert(c);

			var info = new BTreeIndexInfo()
			{
				KeyMaxLength = keyMaxLength,
				RootPageHandle = new(rawHandle)
			};

			return new BTreeIndex(name, indexedField, clusteredIndex, pool, @lock, info);
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
			var c = document.TryGetInt64(
				BarbadosIdentifiers.MetaCollection.CollectionDocumentClusteredIndexPageHandleFieldAbsolute,
				out var clusteredIndexRootHandleRaw
			);
			Debug.Assert(a);
			Debug.Assert(b);
			Debug.Assert(c);

			var clusteredIndex = new BTreeClusteredIndex(pool, new(clusteredIndexRootHandleRaw));
			return new BarbadosCollection(
				collection, new(collectionPageHandleRaw), pool, @lock, clusteredIndex
			);
		}
	}
}
