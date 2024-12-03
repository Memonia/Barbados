using System;

namespace Barbados.StorageEngine.Documents.Exceptions
{
	internal static class BarbadosDocumentInvalidOperationException
	{
		public static T ThrowFieldOfTypeNotFoundOrReturnValue<T>(bool result, T value, BarbadosIdentifier field)
		{
			if (result)
			{
				return value;
			}

			throw new InvalidOperationException($"Document does not contain the field '{field}' of type '{typeof(T)}'");
		}
	}
}
