using System;

namespace Barbados.Documents.RadixTree.Exceptions
{
	internal sealed class RadixTreeSerialisationException : Exception
	{
		public RadixTreeSerialisationException(string message)
			: base(message)
		{

		}

		public RadixTreeSerialisationException(string message, Exception innerException)
			: base(message, innerException)
		{

		}
	}
}
