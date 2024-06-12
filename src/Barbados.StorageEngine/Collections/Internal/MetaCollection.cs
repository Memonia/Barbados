using System;
using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Collections.Internal
{
	internal sealed partial class MetaCollection : AbstractCollection
	{
		private BTreeIndex _index => Indexes[0];
		private readonly BarbadosDocument.Builder _documentBuilder;

		public MetaCollection(
			PageHandle collectionPageHandle,
			PagePool pool,
			LockAutomatic @lock,
			BTreeIndex nameIndex,
			BTreeClusteredIndex clusteredIndex
		) : base(
			BarbadosIdentifiers.Collection.MetaCollection,
			collectionPageHandle,
			pool,
			@lock,
			clusteredIndex
		)
		{
			AddBTreeIndex(nameIndex);
			_documentBuilder = new();
		}

		public bool Find(string collection, out BarbadosDocument document)
		{
			var ids = _index.FindExact(collection).ToArray();
			if (ids.Length > 1)
			{
				throw new BarbadosException(BarbadosExceptionCode.InternalError);
			}

			if (ids.Length != 1)
			{
				document = default!;
				return false;
			}

			return TryRead(ids[0], out document);
		}

		public BarbadosDocument Create(string collection)
		{
			using (Lock.Acquire(LockMode.Write))
			{
				var ch = Pool.Allocate();
				var ih = Pool.Allocate();
				var cpage = new CollectionPage(ch);
				var ipage = new BTreeRootPage(ih);

				Pool.Save(cpage);
				Pool.Save(ipage);

				var collectionDocument = _documentBuilder
					.Add(BarbadosIdentifiers.MetaCollection.CollectionDocumentNameFIeld, collection)
					.Add(BarbadosIdentifiers.MetaCollection.CollectionDocumentPageHandleField, ch.Handle)
					.Add(BarbadosIdentifiers.MetaCollection.CollectionDocumentClusteredIndexPageHandleField, ih.Handle)
					.Build();

				var document = _documentBuilder
					.Add(BarbadosIdentifiers.MetaCollection.CollectionDocumentField, collectionDocument)
					.Build();

				InsertNoLock(document);
				return document;
			}
		}

		public BarbadosDocument CreateIndex(BarbadosDocument document, string field, int keyMaxLength = -1)
		{
			if (keyMaxLength == -1)
			{
				keyMaxLength = Constants.DefaultMaxIndexKeyLength;
			}

			else
			{
				if (keyMaxLength < Constants.MinimalMaxIndexKeyLength || keyMaxLength > Constants.IndexKeyMaxLength)
				{
					throw new ArgumentException(
						$"Allowed custom length is between" +
						$"{Constants.MinimalMaxIndexKeyLength} and {Constants.IndexKeyMaxLength} bytes, " +
						$"got {keyMaxLength}",
						nameof(keyMaxLength)
					);
				}
			}

			using (Lock.Acquire(LockMode.Write))
			{
				var r = document.TryGetString(
					BarbadosIdentifiers.MetaCollection.CollectionDocumentNameFieldAbsolute, out var collection
				);
				Debug.Assert(r);

				if (!document.TryGetDocumentArray(BarbadosIdentifiers.MetaCollection.IndexArrayField, out var indexArray))
				{
					indexArray = [];
				}

				foreach (var index in indexArray)
				{
					r = index.TryGetString(
						BarbadosIdentifiers.MetaCollection.IndexDocumentIndexedFieldField, out var storedIndexedFieldName
					);
					Debug.Assert(r);

					if (field == storedIndexedFieldName)
					{
						throw new BarbadosException(
							BarbadosExceptionCode.IndexAlreadyExists,
							$"Index on '{field}' in collection '{collection}' already exists"
						);
					}
				}

				var ih = Pool.Allocate();
				var ipage = new BTreeRootPage(ih);

				Pool.Save(ipage);

				var indexDocument = _documentBuilder
					.Add(BarbadosIdentifiers.MetaCollection.IndexDocumentIndexedFieldField, field)
					.Add(BarbadosIdentifiers.MetaCollection.IndexDocumentPageHandleField, ih.Handle)
					.Add(BarbadosIdentifiers.MetaCollection.IndexDocumentKeyMaxLengthField, keyMaxLength)
					.Build();

				var updatedIndexesArray = new BarbadosDocument[indexArray.Length + 1];
				indexArray.CopyTo(updatedIndexesArray, 0);
				updatedIndexesArray[^1] = indexDocument;

				var updated = _documentBuilder
					.AddFieldFrom(BarbadosIdentifiers.MetaCollection.CollectionDocumentField, document)
					.Add(BarbadosIdentifiers.MetaCollection.IndexArrayField, updatedIndexesArray)
					.Build();

				if (!TryUpdateNoLock(document.Id, updated))
				{
					throw new BarbadosException(BarbadosExceptionCode.InternalError);
				}

				return indexDocument;
			}
		}

		public void Rename(BarbadosDocument document, string name)
		{
			using (Lock.Acquire(LockMode.Write))
			{
				_documentBuilder
					.Add(BarbadosIdentifiers.MetaCollection.CollectionDocumentNameFieldAbsolute, name)
					.AddFieldFrom(BarbadosIdentifiers.MetaCollection.CollectionDocumentPageHandleFieldAbsolute, document)
					.AddFieldFrom(BarbadosIdentifiers.MetaCollection.CollectionDocumentClusteredIndexPageHandleFieldAbsolute, document);

				if (document.TryGetDocumentArray(BarbadosIdentifiers.MetaCollection.IndexArrayField, out var indexesArray))
				{
					_documentBuilder.Add(BarbadosIdentifiers.MetaCollection.IndexArrayField, indexesArray);
				}

				var updated = _documentBuilder.Build();
				if (!TryUpdateNoLock(document.Id, updated))
				{
					throw new BarbadosException(BarbadosExceptionCode.InternalError);
				}
			}
		}

		public void Remove(BarbadosDocument document)
		{
			if (!TryRemove(document.Id))
			{
				throw new BarbadosException(BarbadosExceptionCode.InternalError);
			}
		}

		public void RemoveIndex(BarbadosDocument document, string field)
		{
			using (Lock.Acquire(LockMode.Write))
			{
				var r = document.TryGetDocumentArray(
					BarbadosIdentifiers.MetaCollection.IndexArrayField, out var indexesArray
				);
				Debug.Assert(r);
				Debug.Assert(indexesArray.Length > 0);

				var updatedIndexesArray = new BarbadosDocument[indexesArray.Length - 1];
				for (int i = 0; i < updatedIndexesArray.Length;)
				{
					var indexDocument = indexesArray[i];
					r = indexDocument.TryGetString(
						BarbadosIdentifiers.MetaCollection.IndexDocumentIndexedFieldField, out var indexedField
					);
					Debug.Assert(r);

					if (indexedField != field)
					{
						updatedIndexesArray[i] = indexDocument;
						i += 1;
					}
				}

				if (updatedIndexesArray.Length > 0)
				{
					_documentBuilder.Add(BarbadosIdentifiers.MetaCollection.IndexArrayField, updatedIndexesArray);
				}

				var updated = _documentBuilder
					.AddFieldFrom(BarbadosIdentifiers.MetaCollection.CollectionDocumentField, document)
					.Build();

				if (!TryUpdateNoLock(document.Id, updated))
				{
					throw new BarbadosException(BarbadosExceptionCode.InternalError);
				}
			}
		}
	}
}
