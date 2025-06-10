using System;

namespace Barbados.StorageEngine.Exceptions
{
	internal static class BarbadosArgumentExceptionHelpers
	{
		public static void ThrowReservedCollectionId(ObjectId collectionId, string paramName)
		{
			if (collectionId.Value < 0)
			{
				throw new ArgumentException(
					$"Collection id '{collectionId}' cannot be used. Values below zero are reserved for internal use", paramName
				);
			}
		}

		public static void ThrowReservedCollectionName(BarbadosDbObjectName collectionName, string paramName)
		{
			if (collectionName.IsReserved())
			{
				throw new ArgumentException(
					$"'{collectionName}' cannot be used. '?<name>' format is reserved for internal use", paramName
				);
			}
		}

		public static ArgumentException NoPrimaryKeyField()
		{
			return new ArgumentException(
				$"Document does not contain a primary key field '{BarbadosDocumentKeys.DocumentId}'"
			);
		}
	}
}
