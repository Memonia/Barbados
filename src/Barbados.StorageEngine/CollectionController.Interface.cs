using System.Collections.Generic;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal partial class CollectionController
	{
		IBarbadosCollection ICollectionController.Get(ObjectId collectionId)
		{
			return Get(collectionId);
		}

		IBarbadosCollection ICollectionController.Get(BarbadosDbObjectName collectionName)
		{
			return Get(collectionName);
		}

		public IEnumerable<string> List()
		{
			yield return BarbadosDbObjects.Collections.MetaCollection;

			using var cursor = _metaFacade.Find(FindOptions.All);
			foreach (var document in cursor)
			{
				yield return document.GetString(BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField);
			}
		}

		public ManagedCollectionFacade Get(ObjectId collectionId)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryGet(collectionId, out var facade))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionId);
			}

			return facade;
		}

		public ManagedCollectionFacade Get(BarbadosDbObjectName collectionName)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryGet(collectionName, out var facade))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionName);
			}

			return facade;
		}

		public void EnsureCreated(BarbadosDbObjectName collectionName)
		{
			EnsureCreated(collectionName, CreateCollectionOptions.Default);
		}

		public void EnsureCreated(BarbadosDbObjectName collectionName, CreateCollectionOptions options)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			TryCreate(collectionName, options);
		}

		public void EnsureDeleted(BarbadosDbObjectName collectionName)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			TryDelete(collectionName);
		}

		public bool Exists(ObjectId collectionId)
		{
			if (collectionId.Value == _metaFacade.Id.Value)
			{
				return true;
			}

			return _metaFacade.TryGetCollectionDocument(collectionId, out _);
		}

		public bool Exists(BarbadosDbObjectName collectionName)
		{
			if (collectionName == BarbadosDbObjects.Collections.MetaCollection)
			{
				return true;
			}

			return _metaFacade.TryGetCollectionId(collectionName, out _);
		}

		public void Rename(ObjectId collectionId, BarbadosDbObjectName replacement)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(replacement, nameof(replacement));
			if (!TryRename(collectionId, replacement))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		public void Delete(ObjectId collectionId)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionId(collectionId, nameof(collectionId));
			if (!TryDelete(collectionId))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionId);
			}
		}

		public void Create(BarbadosDbObjectName collectionName)
		{
			Create(collectionName, CreateCollectionOptions.Default);
		}

		public void Create(BarbadosDbObjectName collectionName, CreateCollectionOptions options)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryCreate(collectionName, options))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionAlreadyExists(collectionName);
			}
		}

		public void Rename(BarbadosDbObjectName collectionName, BarbadosDbObjectName replacement)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(replacement, nameof(replacement));
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryRename(collectionName, replacement))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionName);
			}
		}

		public void Delete(BarbadosDbObjectName collectionName)
		{
			BarbadosArgumentExceptionHelpers.ThrowReservedCollectionName(collectionName, nameof(collectionName));
			if (!TryDelete(collectionName))
			{
				BarbadosCollectionExceptionHelpers.ThrowCollectionDoesNotExist(collectionName);
			}
		}
	}
}
