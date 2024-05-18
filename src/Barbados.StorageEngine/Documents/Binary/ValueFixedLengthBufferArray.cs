using System;
using System.Diagnostics;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal abstract class ValueFixedLengthBufferArray<T> : ValueBuffer
	{
		public T[] Values { get; }
		public int ValueLength { get; }

		public ValueFixedLengthBufferArray(T[] values, ValueTypeMarker marker) : base(marker, true)
		{
			Debug.Assert(!marker.IsVariableLength());

			Values = values;
			ValueLength = marker.GetFixedLength();
		}

		public override int GetLength()
		{
			return sizeof(int) + ValueLength * Values.Length;
		}

		public override void WriteTo(Span<byte> destination)
		{
			var offset = 0;
			ValueBufferRawHelpers.WriteInt32(destination[offset..], Values.Length);

			offset += sizeof(int);
			for (int i = 0; i < Values.Length; ++i)
			{
				WriteValueTo(destination[offset..], i);
				offset += ValueLength;
			}
		}

		protected abstract void WriteValueTo(Span<byte> destination, int index);
	}
}
