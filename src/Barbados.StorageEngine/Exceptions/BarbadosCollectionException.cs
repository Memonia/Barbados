namespace Barbados.StorageEngine.Exceptions
{
	internal static class BarbadosCollectionException
	{
		public static void ThrowCollectionDoesNotExist(ObjectId collectionId)
		{
			throw new BarbadosException(
				BarbadosExceptionCode.CollectionDoesNotExist, $"Collection with id '{collectionId}' does not exist"
			);
		}

		public static void ThrowCollectionDoesNotExist(BarbadosDbObjectName collectionName)
		{
			throw new BarbadosException(
				BarbadosExceptionCode.CollectionDoesNotExist, $"Collection '{collectionName}' does not exist"
			);
		}

		public static void ThrowCollectionAlreadyExists(BarbadosDbObjectName collectionName)
		{
			throw new BarbadosException(
				BarbadosExceptionCode.CollectionAlreadyExists, $"Collection '{collectionName}' already exists"
			);
		}

		public static void ThrowIndexDoesNotExist(ObjectId collectionId, string field)
		{
			throw new BarbadosException(
				BarbadosExceptionCode.IndexDoesNotExist, $"Index for field '{field}' in collection with id '{collectionId}' does not exist"
			);
		}

		public static void ThrowIndexDoesNotExist(BarbadosDbObjectName collectionName, string field)
		{
			throw new BarbadosException(
				BarbadosExceptionCode.IndexDoesNotExist, $"Index for field '{field}' in collection '{collectionName}' does not exist"
			);
		}

		public static void ThrowIndexAlreadyExists(ObjectId collectionId, string field)
		{
			throw new BarbadosException(
				BarbadosExceptionCode.IndexAlreadyExists, $"Index for field '{field}' in collection with id '{collectionId}' already exists"
			);
		}

		public static void ThrowIndexAlreadyExists(BarbadosDbObjectName collectionName, string field)
		{
			throw new BarbadosException(
				BarbadosExceptionCode.IndexAlreadyExists, $"Index for field '{field}' in collection '{collectionName}' already exists"
			);
		}
	}
}
