using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	delegate void ValueBufferWriterDelegate<T>(Span<byte> span, T value);
}
