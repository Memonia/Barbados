using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Collections.Extensions;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed partial class MetaCollectionFacade : BaseBarbadosCollectionFacade, IReadOnlyBarbadosCollection
	{
		public override BarbadosDbObjectName Name => BarbadosDbObjects.Collections.MetaCollection;

		private readonly IndexInfo _nameIndexInfo;
		private readonly BarbadosDocument.Builder _documentBuilder;

		public MetaCollectionFacade(CollectionInfo info, IndexInfo nameIndexInfo, TransactionManager transactionManager)
			: base(info, transactionManager)
		{
			_nameIndexInfo = nameIndexInfo;
			_documentBuilder = new();
		}

		public bool TryGetCollectionId(string collection, out ObjectId id)
		{
			var fo = new FindOptionsBuilder().Eq(collection).Build();
			using var cursor = Find(fo, _nameIndexInfo.Field);
			var doc = cursor.FirstOrDefault();
			if (doc is null)
			{
				id = default!;
				return false;
			}

			id = doc.GetObjectId();
			return true;
		}

		public bool TryGetCollectionDocument(ObjectId id, out BarbadosDocument document)
		{
			var fo = new FindOptionsBuilder().Eq(id.Value).Build();
			using var cursor = Find(fo);
			document = cursor.FirstOrDefault()!;
			return document is not null;
		}

		public ObjectId Create(string collection, CreateCollectionOptions options)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(MetaCollectionId, TransactionMode.ReadWrite);
			var btreeInfo = BTreeContext.CreateBTree(tx);

			var collectionDocument = _documentBuilder
				.Add(BarbadosDocumentKeys.MetaCollection.CollectionDocumentNameField, collection)
				.Add(BarbadosDocumentKeys.MetaCollection.CollectionDocumentPageHandleField, btreeInfo.RootHandle.Handle)
				.Add(BarbadosDocumentKeys.MetaCollection.CollectionDocumentIdGenModeField, (int)options.AutomaticIdGeneratorMode)
				.Build();

			_documentBuilder
				.Add(BarbadosDocumentKeys.MetaCollection.CollectionDocumentField, collectionDocument);

			var document = InsertWithAutomaticId(_documentBuilder);
			TransactionManager.CommitTransaction(tx);
			return document.GetObjectId();
		}

		public BarbadosDocument CreateIndex(BarbadosDocument document, BarbadosKey field)
		{
			var collection = document.GetString(BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField);
			if (!document.TryGetDocumentArray(BarbadosDocumentKeys.MetaCollection.IndexArrayField, out var indexArray))
			{
				indexArray = [];
			}

			foreach (var index in indexArray)
			{
				var storedIndexedFieldName = index.GetString(BarbadosDocumentKeys.MetaCollection.IndexDocumentIndexedFieldField);
				if (field == storedIndexedFieldName)
				{
					BarbadosCollectionExceptionHelpers.ThrowIndexAlreadyExists(collection, field.ToString());
				}
			}

			using var tx = TransactionManager.GetAutomaticTransaction(MetaCollectionId, TransactionMode.ReadWrite);
			var btreeInfo = BTreeContext.CreateBTree(tx);

			var indexDocument = _documentBuilder
				.Add(BarbadosDocumentKeys.MetaCollection.IndexDocumentIndexedFieldField, field.ToString())
				.Add(BarbadosDocumentKeys.MetaCollection.IndexDocumentPageHandleField, btreeInfo.RootHandle.Handle)
				.Build();

			var updatedIndexesArray = new BarbadosDocument[indexArray.Length + 1];
			indexArray.CopyTo(updatedIndexesArray, 0);
			updatedIndexesArray[^1] = indexDocument;

			var updated = _documentBuilder
				.AddObjectId(document.GetObjectId())
				.AddFrom(BarbadosDocumentKeys.MetaCollection.CollectionDocumentField, document)
				.Add(BarbadosDocumentKeys.MetaCollection.IndexArrayField, updatedIndexesArray)
				.Build();

			if (!TryUpdate(updated))
			{
				throw new BarbadosInternalErrorException();
			}

			TransactionManager.CommitTransaction(tx);
			return indexDocument;
		}

		public void Rename(BarbadosDocument document, string name)
		{
			_documentBuilder
				.AddObjectId(document.GetObjectId())
				.Add(BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField, name)
				.AddFrom(BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentPageHandleField, document)
				.AddFrom(BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentIdGenModeField, document);

			if (document.TryGetDocumentArray(BarbadosDocumentKeys.MetaCollection.IndexArrayField, out var indexesArray))
			{
				_documentBuilder.Add(BarbadosDocumentKeys.MetaCollection.IndexArrayField, indexesArray);
			}

			var updated = _documentBuilder.Build();
			if (!TryUpdate(updated))
			{
				throw new BarbadosInternalErrorException();
			}
		}

		public void RemoveIndex(BarbadosDocument document, BarbadosKey field)
		{
			var indexArray = document.GetDocumentArray(BarbadosDocumentKeys.MetaCollection.IndexArrayField);
			Debug.Assert(indexArray.Length > 0);

			var updatedIndexesArray = new BarbadosDocument[indexArray.Length - 1];
			for (int i = 0; i < updatedIndexesArray.Length;)
			{
				var indexDocument = indexArray[i];
				var indexField = indexDocument.GetString(BarbadosDocumentKeys.MetaCollection.IndexDocumentIndexedFieldField);
				if (indexField != field)
				{
					updatedIndexesArray[i] = indexDocument;
					i += 1;
				}
			}

			if (updatedIndexesArray.Length > 0)
			{
				_documentBuilder.Add(BarbadosDocumentKeys.MetaCollection.IndexArrayField, updatedIndexesArray);
			}

			var updated = _documentBuilder
				.AddObjectId(document.GetObjectId())
				.AddFrom(BarbadosDocumentKeys.MetaCollection.CollectionDocumentField, document)
				.Build();

			if (!TryUpdate(updated))
			{
				throw new BarbadosInternalErrorException();
			}
		}

		public bool IndexExists(BarbadosKey field)
		{
			return field == BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField;
		}

		protected override IndexInfo GetIndexInfo(BarbadosKey field)
		{
			if (field != BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField)
			{
				BarbadosCollectionExceptionHelpers.ThrowIndexDoesNotExist(Id, field.ToString());
			}

			return _nameIndexInfo;
		}

		protected override IEnumerable<IndexInfo> EnumerateIndexes()
		{
			yield return _nameIndexInfo;
		}
	}
}
