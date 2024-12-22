using System;
using System.Diagnostics;

namespace Barbados.Documents.Serialisation.Values
{
	internal sealed class FixedLengthTypeValueBuffer<T> : ValueBuffer
	{
		public T Value { get; }
		public int ValueLength { get; }

		private readonly ValueBufferWriterDelegate<T> _writer;

		public FixedLengthTypeValueBuffer(T value, ValueTypeMarker marker, ValueBufferWriterDelegate<T> writer) : base(marker)
		{
			Debug.Assert(!marker.IsArray());
			Debug.Assert(!marker.IsVariableLengthTypeValue());

			Value = value;
			ValueLength = marker.GetFixedLengthTypeLength();
			_writer = writer;
		}

		public override int GetLength()
		{
			return ValueLength;
		}

		public override object GetValue()
		{
			return Value!;
		}

		public override void WriteTo(Span<byte> destination)
		{
			_writer(destination, Value);
		}

		public override string ToString()
		{
			return Value?.ToString()!;
		}
	}
}
