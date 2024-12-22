using System;

namespace Barbados.Documents.Serialisation.Values
{
	internal interface IValueBufferSpanComparer
	{
		int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y);
	}
}
