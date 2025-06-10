using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class CollectionMetadataService
	{
		private readonly ObjectId _collectionId;
		private readonly MetaCollectionFacade _metaFacade;

		public CollectionMetadataService(MetaCollectionFacade metaFacade, ObjectId collectionId)
		{
			_collectionId = collectionId;
			_metaFacade = metaFacade;
		}

		public string GetName()
		{
			if (!_metaFacade.TryGetCollectionDocument(_collectionId, out var document))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.CollectionDoesNotExist, $"Collection with id {_collectionId} no longer exists"
				);
			}

			return document.GetString(BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField);
		}
	}
}
