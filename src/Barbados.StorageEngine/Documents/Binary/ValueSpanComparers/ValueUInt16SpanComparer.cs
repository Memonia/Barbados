using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueSpanComparers
{
	internal sealed class ValueUInt16SpanComparer : IValueSpanComparer
	{
		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			return ValueBufferRawHelpers.ReadUInt16(x).CompareTo(ValueBufferRawHelpers.ReadUInt16(y));
		}
	}
}
