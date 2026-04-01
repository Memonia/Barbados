using System;

namespace Barbados.Documents.Exceptions
{
	internal static class BarbadosArgumentExceptionHelpers
	{
		public static void ThrowKeyAlreadyExists(string key, string paramName, Exception? innerException = null)
		{
			throw new ArgumentException($"Key '{key}' already exists", paramName, innerException);
		}

		public static void ThrowKeyConflictsWithAncestor(string key, string ancestor, string paramName)
		{
			throw new ArgumentException(
				$"Key '{key}' conflicts with an existing value at key '{ancestor}'", paramName
			);
		}
	}
}
