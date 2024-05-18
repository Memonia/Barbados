using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueSpanComparers
{
	internal sealed class ValueStringSpanComparer : IValueSpanComparer
	{
		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			return x.SequenceCompareTo(y);
		}
	}
}
