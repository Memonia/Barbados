using System;

namespace Barbados.Documents.Serialisation.Values
{
	internal sealed class FixedLengthTypeValueBufferSpanComparer<T> : IValueBufferSpanComparer
		where T : IComparable<T>
	{
		private readonly ValueBufferReaderDelegate<T> _reader;

		public FixedLengthTypeValueBufferSpanComparer(ValueBufferReaderDelegate<T> reader)
		{
			_reader = reader;
		}

		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			return _reader(x).CompareTo(_reader(y));
		}
	}
}
