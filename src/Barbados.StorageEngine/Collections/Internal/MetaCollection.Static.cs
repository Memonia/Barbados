using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Collections.Internal
{
	internal partial class MetaCollection
	{
		public static BTreeIndex CreateIndexInstance(string collection, BarbadosDocument document, BarbadosController controller)
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

			var indexName = $"{collection}.{indexedField}";
			return new BTreeIndex(indexName, collection, indexedField, controller, info);
		}

		public static BarbadosCollection CreateCollectionInstance(BarbadosDocument document, BarbadosController controller)
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

			var indexes = new List<BTreeIndex>();
			var clusteredIndex = new BTreeClusteredIndex(controller, new(clusteredIndexRootHandleRaw));
			if (document.TryGetDocumentArray(BarbadosIdentifiers.MetaCollection.IndexArrayField, out var indexesArray))
			{
				foreach (var indexDocument in indexesArray)
				{
					var index = CreateIndexInstance(collection, indexDocument, controller);
					indexes.Add(index);
				}
			}

			return new BarbadosCollection(
				collection, controller, new(collectionPageHandleRaw), indexes, clusteredIndex
			);
		}
	}
}
