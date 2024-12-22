using System;

namespace Barbados.Documents.Serialisation.Values
{
	delegate T ValueBufferReaderDelegate<T>(ReadOnlySpan<byte> source);
}
