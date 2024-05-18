using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueSpanComparers
{
	internal sealed class ValueInt32SpanComparer : IValueSpanComparer
	{
		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			return ValueBufferRawHelpers.ReadInt32(x).CompareTo(ValueBufferRawHelpers.ReadInt32(y));
		}
	}
}
