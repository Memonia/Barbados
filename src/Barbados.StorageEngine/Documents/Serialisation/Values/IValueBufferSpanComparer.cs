using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal interface IValueBufferSpanComparer
	{
		int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y);
	}
}
