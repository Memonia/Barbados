using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueSpanComparers
{
	internal sealed class ValueUInt64SpanComparer : IValueSpanComparer
	{
		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			return ValueBufferRawHelpers.ReadUInt64(x).CompareTo(ValueBufferRawHelpers.ReadUInt64(y));
		}
	}
}
