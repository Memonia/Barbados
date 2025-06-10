using System;

namespace Barbados.Documents.RadixTree.Values
{
	internal interface IValueBufferSpanComparer
	{
		int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y);
	}
}
