using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	delegate T ValueBufferReaderDelegate<T>(ReadOnlySpan<byte> source); 
}
