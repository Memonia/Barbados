using System;

namespace Barbados.Documents.RadixTree.Exceptions
{
	internal sealed class RadixTreeDuplicateKeyException : Exception
	{
		public RadixTreeDuplicateKeyException()
			: base()
		{

		}

		public RadixTreeDuplicateKeyException(string message)
			: base(message)
		{

		}

		public RadixTreeDuplicateKeyException(string message, Exception innerException)
			: base(message, innerException)
		{

		}
	}
}
