using System;
using System.Diagnostics;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal abstract class ValueVariableLengthBuffer<T> : ValueBuffer
	{
		public T Value { get; }
		public int ValueLength { get; }

		public ValueVariableLengthBuffer(T value, int valueLength, ValueTypeMarker marker) : base(marker, false)
		{
			Debug.Assert(marker.IsVariableLength());

			Value = value;
			ValueLength = valueLength;
		}

		public override int GetLength()
		{
			return sizeof(int) + ValueLength;
		}

		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteInt32(destination, ValueLength);
			WriteValueTo(destination[sizeof(int)..]);
		}

		public abstract void WriteValueTo(Span<byte> destination);
	}
}
