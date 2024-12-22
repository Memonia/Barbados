using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal partial class CollectionController
	{
		void ICollectionController.EnsureCreated(BarbadosDbObjectName collectionName)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			TryCreate(collectionName);
		}

		void ICollectionController.EnsureDeleted(BarbadosDbObjectName collectionName)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			TryDelete(collectionName);
		}

		IEnumerable<string> ICollectionController.List()
		{
			yield return BarbadosDbObjects.Collections.MetaCollection;
			foreach (var document in _metaFacade.GetCursor())
			{
				yield return document.GetString(
					BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField
				);
			}
		}

		bool ICollectionController.Exists(ObjectId collectionId)
		{
			if (collectionId.Value == _metaFacade.Id.Value)
			{
				return true;
			}

			return _metaFacade.TryRead(collectionId, BarbadosKeySelector.SelectNone, out _);
		}

		bool ICollectionController.Exists(BarbadosDbObjectName collectionName)
		{
			if (collectionName == BarbadosDbObjects.Collections.MetaCollection)
			{
				return true;
			}

			return _metaFacade.Find(collectionName, out _);
		}

		IBarbadosCollection ICollectionController.Get(ObjectId collectionId)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryGet(collectionId, out var facade))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionId);
			}

			return facade;
		}

		IBarbadosCollection ICollectionController.Get(BarbadosDbObjectName collectionName)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryGet(collectionName, out var facade))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionName);
			}

			return facade;
		}

		void ICollectionController.Rename(ObjectId collectionId, BarbadosDbObjectName replacement)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			BarbadosArgumentException.ThrowReservedCollectionName(replacement, nameof(replacement));
			if (!TryRename(collectionId, replacement))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		void ICollectionController.Delete(ObjectId collectionId)
		{
			BarbadosArgumentException.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryDelete(collectionId))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		void ICollectionController.Create(BarbadosDbObjectName collectionName)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryCreate(collectionName))
			{
				BarbadosCollectionException.ThrowCollectionAlreadyExists(collectionName);
			}
		}

		void ICollectionController.Rename(BarbadosDbObjectName collectionName, BarbadosDbObjectName replacement)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(replacement, nameof(replacement));
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryRename(collectionName, replacement))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionName);
			}
		}

		void ICollectionController.Delete(BarbadosDbObjectName collectionName)
		{
			BarbadosArgumentException.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryDelete(collectionName))
			{
				BarbadosCollectionException.ThrowCollectionDoesNotExist(collectionName);
			}
		}
	}
}
