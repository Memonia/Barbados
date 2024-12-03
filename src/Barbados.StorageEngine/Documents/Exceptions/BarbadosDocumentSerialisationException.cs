using System;

namespace Barbados.StorageEngine.Documents.Exceptions
{
	internal sealed class BarbadosDocumentSerialisationException : Exception
	{
		public BarbadosDocumentSerialisationException(string message)
			: base(message)
		{

		}

		public BarbadosDocumentSerialisationException(string message, Exception innerException)
			: base(message, innerException)
		{

		}
	}
}
