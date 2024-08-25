using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed partial class MetaCollectionFacade : AbstractCollectionFacade, IReadOnlyBarbadosCollection
	{
		private readonly BTreeIndexFacade _nameIndexFacade;
		private readonly BarbadosDocument.Builder _documentBuilder;

		public MetaCollectionFacade(
			TransactionManager transactionManager,
			BTreeClusteredIndexFacade clusteredIndexFacade,
			BTreeIndexFacade nameIndexFacade
		) : base(MetaCollectionId, transactionManager, clusteredIndexFacade)
		{
			_nameIndexFacade = nameIndexFacade;
			_documentBuilder = new();
		}

		public bool Find(string collection, out ObjectId id)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(MetaCollectionId, TransactionMode.Read);
			var ids = _nameIndexFacade.FindExact(collection).ToList();
			if (ids.Count > 1)
			{
				throw new BarbadosInternalErrorException();
			}

			if (ids.Count != 1)
			{
				id = default!;
				return false;
			}

			id = ids[0];
			return true;
		}

		public ObjectId Create(string collection)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(MetaCollectionId, TransactionMode.ReadWrite);
			var h = tx.AllocateHandle();
			var page = new CollectionPage(h);

			tx.Save(page);
			var collectionDocument = _documentBuilder
				.Add(CommonIdentifiers.MetaCollection.CollectionDocumentNameField, collection)
				.Add(CommonIdentifiers.MetaCollection.CollectionDocumentPageHandleField, h.Handle)
				.Build();

			var document = _documentBuilder
				.Add(CommonIdentifiers.MetaCollection.CollectionDocumentField, collectionDocument)
				.Build();

			var id = Insert(document);
			TransactionManager.CommitTransaction(tx);
			return id;
		}

		public BarbadosDocument CreateIndex(BarbadosDocument document, string field)
		{
			return CreateIndex(document, field, Constants.DefaultMaxIndexKeyLength);
		}

		public BarbadosDocument CreateIndex(BarbadosDocument document, string field, int keyMaxLength)
		{
			if (keyMaxLength < Constants.MinIndexKeyMaxLength || keyMaxLength > Constants.IndexKeyMaxLength)
			{
				throw new ArgumentException(
					$"Allowed custom length is between " +
					$"{Constants.MinIndexKeyMaxLength} and " +
					$"{Constants.IndexKeyMaxLength} bytes, got {keyMaxLength}",
					nameof(keyMaxLength)
				);
			}

			var collection = document.GetString(CommonIdentifiers.MetaCollection.AbsCollectionDocumentNameField);
			if (!document.TryGetDocumentArray(CommonIdentifiers.MetaCollection.IndexArrayField, out var indexArray))
			{
				indexArray = [];
			}

			foreach (var index in indexArray)
			{
				var storedIndexedFieldName = index.GetString(CommonIdentifiers.MetaCollection.IndexDocumentIndexedFieldField);
				if (field == storedIndexedFieldName)
				{
					throw new BarbadosException(
						BarbadosExceptionCode.IndexAlreadyExists,
						$"Index on '{field}' in collection '{collection}' already exists"
					);
				}
			}

			using var tx = TransactionManager.GetAutomaticTransaction(MetaCollectionId, TransactionMode.ReadWrite);
			var ih = tx.AllocateHandle();
			var ipage = new BTreeRootPage(ih);

			tx.Save(ipage);

			var indexDocument = _documentBuilder
				.Add(CommonIdentifiers.MetaCollection.IndexDocumentIndexedFieldField, field)
				.Add(CommonIdentifiers.MetaCollection.IndexDocumentPageHandleField, ih.Handle)
				.Add(CommonIdentifiers.MetaCollection.IndexDocumentKeyMaxLengthField, keyMaxLength)
				.Build();

			var updatedIndexesArray = new BarbadosDocument[indexArray.Length + 1];
			indexArray.CopyTo(updatedIndexesArray, 0);
			updatedIndexesArray[^1] = indexDocument;

			var updated = _documentBuilder
				.AddFieldFrom(CommonIdentifiers.MetaCollection.CollectionDocumentField, document)
				.Add(CommonIdentifiers.MetaCollection.IndexArrayField, updatedIndexesArray)
				.Build();

			if (!TryUpdate(document.Id, updated))
			{
				throw new BarbadosInternalErrorException();
			}

			TransactionManager.CommitTransaction(tx);
			return indexDocument;
		}

		public void Rename(BarbadosDocument document, string name)
		{
			_documentBuilder
				.Add(CommonIdentifiers.MetaCollection.AbsCollectionDocumentNameField, name)
				.AddFieldFrom(CommonIdentifiers.MetaCollection.AbsCollectionDocumentPageHandleField, document);

			if (document.TryGetDocumentArray(CommonIdentifiers.MetaCollection.IndexArrayField, out var indexesArray))
			{
				_documentBuilder.Add(CommonIdentifiers.MetaCollection.IndexArrayField, indexesArray);
			}

			var updated = _documentBuilder.Build();
			if (!TryUpdate(document.Id, updated))
			{
				throw new BarbadosInternalErrorException();
			}
		}

		public void RemoveIndex(BarbadosDocument document, string field)
		{
			var indexArray = document.GetDocumentArray(CommonIdentifiers.MetaCollection.IndexArrayField);
			Debug.Assert(indexArray.Length > 0);

			var updatedIndexesArray = new BarbadosDocument[indexArray.Length - 1];
			for (int i = 0; i < updatedIndexesArray.Length;)
			{
				var indexDocument = indexArray[i];
				var indexField = indexDocument.GetString(CommonIdentifiers.MetaCollection.IndexDocumentIndexedFieldField);
				if (indexField != field)
				{
					updatedIndexesArray[i] = indexDocument;
					i += 1;
				}
			}

			if (updatedIndexesArray.Length > 0)
			{
				_documentBuilder.Add(CommonIdentifiers.MetaCollection.IndexArrayField, updatedIndexesArray);
			}

			var updated = _documentBuilder
				.AddFieldFrom(CommonIdentifiers.MetaCollection.CollectionDocumentField, document)
				.Build();

			if (!TryUpdate(document.Id, updated))
			{
				throw new BarbadosInternalErrorException();
			}
		}

		protected override IEnumerable<BTreeIndexFacade> EnumerateIndexes()
		{
			yield return _nameIndexFacade;
		}
	}
}
