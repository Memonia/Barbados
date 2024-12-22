using System;

namespace Barbados.Documents.Exceptions
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
