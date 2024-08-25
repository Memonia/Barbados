using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal sealed class CollectionControllerService
	{
		private static readonly ValueSelector _collectionNameSelector =
			new(CommonIdentifiers.MetaCollection.AbsCollectionDocumentNameField);

		private readonly MetaCollectionFacade _metaFacade;

		public CollectionControllerService(MetaCollectionFacade metaFacade)
		{
			_metaFacade = metaFacade;
		}

		public string GetCollectionName(ObjectId id)
		{
			if (!_metaFacade.TryRead(id, _collectionNameSelector, out var document))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.CollectionDoesNotExist, $"Collection with id {id} no longer exists"
				);
			}

			return document.GetString(_collectionNameSelector[0]);
		}
	}
}
