using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueSpanComparers
{
	internal sealed class ValueInt8SpanComparer : IValueSpanComparer
	{
		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			return ValueBufferRawHelpers.ReadInt8(x).CompareTo(ValueBufferRawHelpers.ReadInt8(y));
		}
	}
}
