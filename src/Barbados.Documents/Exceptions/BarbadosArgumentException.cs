using System;

namespace Barbados.Documents.Exceptions
{
	internal static class BarbadosArgumentException
	{
		public static void ThrowDocumentKeyWhenValueExpected(BarbadosKey key, string paramName)
		{
			if (key.IsDocument)
			{
				throw new ArgumentException($"Expected a value key, got a document '{key}' instead", paramName);
			}
		}
	}
}
