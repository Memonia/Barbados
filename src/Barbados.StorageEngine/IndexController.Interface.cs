using System.Collections.Generic;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal partial class IndexController
	{
		public IEnumerable<string> ListIndexed(ObjectId collectionId)
		{
			if (collectionId.Value == _metaFacade.Id.Value)
			{
				yield return BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField.ToString();
				yield break;
			}

			if (!_metaFacade.TryGetCollectionDocument(collectionId, out var document))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionId);
			}

			foreach (var indexDocument in MetaCollectionFacade.EnumerateIndexDocuments(document))
			{
				yield return indexDocument.GetString(
					BarbadosDocumentKeys.MetaCollection.IndexDocumentIndexedFieldField
				);
			}
		}

		public IEnumerable<string> ListIndexed(BarbadosDbObjectName collectionName)
		{
			if (collectionName == BarbadosDbObjects.Collections.MetaCollection)
			{
				yield return BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField.ToString();
				yield break;
			}

			if (!_metaFacade.TryGetCollectionId(collectionName, out var id))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionName);
			}

			foreach (var indexed in ((IIndexController)this).ListIndexed(id))
			{
				yield return indexed;
			}
		}

		public void EnsureCreated(ObjectId collectionId, string field)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!Exists(collectionId, field))
			{
				TryCreate(collectionId, field);
			}
		}

		public void EnsureDeleted(ObjectId collectionId, string field)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (Exists(collectionId, field))
			{
				TryDelete(collectionId, field);
			}
		}

		public void EnsureCreated(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!((IIndexController)this).Exists(collectionName, field))
			{
				TryCreate(collectionName, field);
			}
		}

		public void EnsureDeleted(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (((IIndexController)this).Exists(collectionName, field))
			{
				TryDelete(collectionName, field);
			}
		}

		public bool Exists(ObjectId collectionId, string field)
		{
			if (collectionId.Value == MetaCollectionFacade.MetaCollectionId.Value)
			{
				return field == BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField;
			}

			foreach (var f in ((IIndexController)this).ListIndexed(collectionId))
			{
				if (f == field)
				{
					return true;
				}
			}

			return false;
		}

		public bool Exists(BarbadosDbObjectName collectionName, string field)
		{
			if (collectionName == BarbadosDbObjects.Collections.MetaCollection)
			{
				return field == BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField;
			}

			foreach (var f in ((IIndexController)this).ListIndexed(collectionName))
			{
				if (f == field)
				{
					return true;
				}
			}
			return false;
		}

		public void Create(ObjectId collectionId, string field)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryCreate(collectionId, field))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		public void Delete(ObjectId collectionId, string field)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryDelete(collectionId, field))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		public void Create(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryCreate(collectionName, field))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionName);
			}
		}

		public void Delete(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryDelete(collectionName, field))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionName);
			}
		}
	}
}
