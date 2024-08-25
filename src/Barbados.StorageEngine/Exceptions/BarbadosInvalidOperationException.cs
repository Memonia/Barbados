using System;

namespace Barbados.StorageEngine.Exceptions
{
	internal static class BarbadosInvalidOperationException
	{
		public static T ThrowFieldOfTypeNotFoundIfResultFalse<T>(bool result, T value, BarbadosIdentifier field, Type type)
		{
			if (result)
			{
				return value;
			}

			throw new InvalidOperationException($"Document does not contain the field '{field}' of type '{type}'");
		}
	}
}
