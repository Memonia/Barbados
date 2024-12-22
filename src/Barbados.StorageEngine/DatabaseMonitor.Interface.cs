using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal partial class DatabaseMonitor
	{
		public IReadOnlyBarbadosCollection GetInternalCollection(BarbadosDbObjectName collectionName)
		{
			if (collectionName == BarbadosDbObjects.Collections.MetaCollection)
			{
				return _meta;
			}

			throw new BarbadosException(
				BarbadosExceptionCode.CollectionDoesNotExist, $"Internal collection '{collectionName}' does not exist"
			);
		}
	}
}
