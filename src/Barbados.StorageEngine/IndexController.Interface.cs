using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine
{
	internal partial class IndexController
	{
		void IIndexController.EnsureCreated(ObjectId collectionId, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!((IIndexController)this).Exists(collectionId, field))
			{
				TryCreate(collectionId, field, maxKeyLength: -1, useDefault: true);
			}
		}

		void IIndexController.EnsureCreated(ObjectId collectionId, string field, int maxKeyLength)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!((IIndexController)this).Exists(collectionId, field))
			{
				TryCreate(collectionId, field, maxKeyLength, useDefault: false);
			}
		}

		void IIndexController.EnsureDeleted(ObjectId collectionId, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (((IIndexController)this).Exists(collectionId, field))
			{
				TryDelete(collectionId, field);
			}
		}

		void IIndexController.EnsureCreated(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!((IIndexController)this).Exists(collectionName, field))
			{
				TryCreate(collectionName, field, maxKeyLength: -1, useDefault: true);
			}
		}

		void IIndexController.EnsureCreated(BarbadosDbObjectName collectionName, string field, int maxKeyLength)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!((IIndexController)this).Exists(collectionName, field))
			{
				TryCreate(collectionName, field, maxKeyLength, useDefault: false);
			}
		}

		void IIndexController.EnsureDeleted(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (((IIndexController)this).Exists(collectionName, field))
			{
				TryDelete(collectionName, field);
			}
		}

		bool IIndexController.Exists(ObjectId collectionId, string field)
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

		bool IIndexController.Exists(BarbadosDbObjectName collectionName, string field)
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

		IEnumerable<string> IIndexController.ListIndexed(ObjectId collectionId)
		{
			if (collectionId.Value == _metaFacade.Id.Value)
			{
				yield return BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField.ToString();
				yield break;
			}

			if (!_metaFacade.TryRead(collectionId, BarbadosKeySelector.SelectAll, out var document))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionId);
			}

			foreach (var indexDocument in MetaCollectionFacade.EnumerateIndexDocuments(document))
			{
				yield return indexDocument.GetString(
					BarbadosDocumentKeys.MetaCollection.IndexDocumentIndexedFieldField
				);
			}
		}

		IEnumerable<string> IIndexController.ListIndexed(BarbadosDbObjectName collectionName)
		{
			if (collectionName == BarbadosDbObjects.Collections.MetaCollection)
			{
				yield return BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField.ToString();
				yield break;
			}

			if (!_metaFacade.Find(collectionName, out var id))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionName);
			}

			foreach (var indexed in ((IIndexController)this).ListIndexed(id))
			{
				yield return indexed;
			}
		}

		IReadOnlyBTreeIndex IIndexController.Get(ObjectId collectionId, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryGet(collectionId, field, out var facade))
			{
				if (!_metaFacade.TryRead(collectionId, BarbadosKeySelector.SelectNone, out _))
				{
					BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionId);
				}

				BarbadosCollectionException.ThrowIndexDoesNotExist(collectionId, field);
			}

			return facade;
		}

		IReadOnlyBTreeIndex IIndexController.Get(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryGet(collectionName, field, out var facade))
			{
				if (!_metaFacade.Find(collectionName, out _))
				{
					BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionName);
				}

				BarbadosCollectionException.ThrowIndexDoesNotExist(collectionName, field);
			}

			return facade;
		}

		void IIndexController.Create(ObjectId collectionId, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryCreate(collectionId, field, maxKeyLength: -1, useDefault: true))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		void IIndexController.Create(ObjectId collectionId, string field, int maxKeyLength)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryCreate(collectionId, field, maxKeyLength, useDefault: false))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		void IIndexController.Delete(ObjectId collectionId, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryDelete(collectionId, field))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		void IIndexController.Create(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryCreate(collectionName, field, maxKeyLength: -1, useDefault: true))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionName);
			}
		}

		void IIndexController.Create(BarbadosDbObjectName collectionName, string field, int maxKeyLength)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryCreate(collectionName, field, maxKeyLength, useDefault: false))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionName);
			}
		}

		void IIndexController.Delete(BarbadosDbObjectName collectionName, string field)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryDelete(collectionName, field))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionName);
			}
		}
	}
}
