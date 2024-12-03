using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values.ValueBufferSpanComparers
{
	internal sealed class ValueStringBufferSpanComparer : IValueBufferSpanComparer
	{
		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			return x[sizeof(int)..].SequenceCompareTo(y[sizeof(int)..]);
		}
	}
}
