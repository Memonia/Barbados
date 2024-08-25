using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal partial class DatabaseMonitor
	{
		public IReadOnlyBarbadosCollection GetInternalCollection(BarbadosIdentifier collectionName)
		{
			if (collectionName.Identifier == CommonIdentifiers.Collections.MetaCollection.Identifier)
			{
				return _meta;
			}

			throw new BarbadosException(
				BarbadosExceptionCode.CollectionDoesNotExist, $"Internal collection '{collectionName}' does not exist"
			);
		}
	}
}
