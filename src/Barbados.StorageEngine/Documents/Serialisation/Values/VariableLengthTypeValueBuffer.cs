using System;
using System.Diagnostics;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal sealed class VariableLengthTypeValueBuffer<T> : ValueBuffer
	{
		public T Value { get; }
		public int ValueLength { get; }

		private readonly ValueBufferWriterDelegate<T> _writer;

		public VariableLengthTypeValueBuffer(T value, int valueLength, ValueTypeMarker marker, ValueBufferWriterDelegate<T> writer) : base(marker)
		{
			Debug.Assert(marker.IsVariableLengthTypeValue());

			Value = value;
			ValueLength = valueLength;
			_writer = writer;
		}

		public override int GetLength()
		{
			return sizeof(int) + ValueLength;
		}

		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteInt32(destination, ValueLength);
			_writer(destination[sizeof(int)..], Value);
		}

		public override string ToString()
		{
			return Value?.ToString()!;
		}
	}
}
