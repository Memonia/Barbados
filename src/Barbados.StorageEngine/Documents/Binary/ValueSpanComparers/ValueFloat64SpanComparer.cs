using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueSpanComparers
{
	internal sealed class ValueFloat64SpanComparer : IValueSpanComparer
	{
		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			return ValueBufferRawHelpers.ReadFloat64(x).CompareTo(ValueBufferRawHelpers.ReadFloat64(y));
		}
	}
}
