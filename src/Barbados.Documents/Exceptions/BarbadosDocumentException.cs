using System;

namespace Barbados.Documents.Exceptions
{
	internal static class BarbadosDocumentException
	{
		public static T ThrowElementOfTypeNotFoundOrReturnValue<T>(bool result, T value, BarbadosKey key)
		{
			if (result)
			{
				return value;
			}

			throw new InvalidOperationException($"Document does not contain element '{key}' of type '{typeof(T)}'");
		}
	}
}
