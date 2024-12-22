using System;

namespace Barbados.Documents.Serialisation.Values
{
	delegate void ValueBufferWriterDelegate<T>(Span<byte> span, T value);
}
