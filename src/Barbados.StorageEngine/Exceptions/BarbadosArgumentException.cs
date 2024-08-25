using System;

namespace Barbados.StorageEngine.Exceptions
{
	internal static class BarbadosArgumentException
	{
		public static void ThrowGroupIdentifierWhenFieldExpected(BarbadosIdentifier identifier, string paramName)
		{
			if (identifier.IsGroup)
			{
				throw new ArgumentException(
					$"Expected a field identifier, got a group '{identifier}'", paramName
				);
			}
		}

		public static void ThrowFieldIdentifierWhenGroupExpected(BarbadosIdentifier identifier, string paramName)
		{
			if (!identifier.IsGroup)
			{
				throw new ArgumentException(
					$"Expected a group identifier, got a field '{identifier}'", paramName
				);
			}
		}

		public static void ThrowReservedCollectionId(ObjectId collectionId, string paramName)
		{
			if (collectionId.Value < 0)
			{
				throw new ArgumentException(
					$"Collection id '{collectionId}' cannot be used. Values below zero are reserved for internal use",
					paramName
				);
			}
		}

		public static void ThrowReservedCollectionName(BarbadosIdentifier collectionName, string paramName)
		{
			if (collectionName.IsReserved)
			{
				throw new ArgumentException(
					$"'{collectionName}' cannot be used. '?<name>' format is reserved for internal use",
					paramName
				);
			}
		}
	}
}
