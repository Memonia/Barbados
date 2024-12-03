using System;
using System.Diagnostics;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal sealed class FixedLengthTypeArrayBuffer<T> : ValueBuffer
	{
		public T[] Values { get; }
		public int ValueLength { get; }

		private readonly ValueBufferWriterDelegate<T> _writer;

		public FixedLengthTypeArrayBuffer(T[] values, ValueTypeMarker marker, ValueBufferWriterDelegate<T> writer) : base(marker)
		{
			Debug.Assert(marker.IsArray());
			Debug.Assert(!marker.IsVariableLengthTypeArray());

			Values = values;
			ValueLength = marker.GetFixedLengthTypeLength();
			_writer = writer;
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
				_writer(destination[offset..], Values[i]);
				offset += ValueLength;
			}
		}

		public override string ToString()
		{
			return $"[{string.Join(", ", Values)}]";
		}
	}
}
