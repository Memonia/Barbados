using System;

namespace Barbados.Documents.RadixTree.Values
{
	delegate T ValueBufferReaderDelegate<T>(ReadOnlySpan<byte> source);
}
