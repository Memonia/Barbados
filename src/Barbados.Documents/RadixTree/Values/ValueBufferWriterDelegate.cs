using System;

namespace Barbados.Documents.RadixTree.Values
{
	delegate void ValueBufferWriterDelegate<T>(Span<byte> span, T value);
}
