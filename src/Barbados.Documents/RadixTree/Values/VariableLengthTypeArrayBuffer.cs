using System;

namespace Barbados.Documents.RadixTree.Values
{
	internal sealed class VariableLengthTypeArrayBuffer<T> : ValueBuffer
	{
		/* Format:
		 *   I32 number of offsets
		 *   [I32 offset] offsets relative to the start of the first buffer
		 *   [VAR buffers]
		 *   
		 * Example (assuming each character is a single byte):
		 *  ["cat", "dog", "penguin"] => 3, 3, 6, 13, cat, dog, pengiun 
		 * 
		 * Notice how the offsets are shifted. First offset is at the start of "dog" and the last offset
		 * is one byte after "penguin". 
		 * 
		 * If we want to access the first string, we start on the first byte after the last offset 
		 * and start reading the number of bytes equal to the first offset.
		 *
		 * If we want to access the second string, we find its length (first offset - second offset)
		 * and start reading from the relative offset at index 1.
		 * 
		 * For the third string the length is (second offset - third offset) and we start reading from 
		 * relative offset at index 2.
		 * 
		 * While this is harder to work with, the format enables random access to the elements of the array.
		 * 
		 * If we were to store the length of each element, we would have to sum them all on each read up 
		 * to nth index in order to find the location of the element. Alternatively, if we were to store 
		 * the absolute offsets, we wouldn't be able to tell how long the last string is without any extra
		 * book-keeping.
		 */

		public T[] Values { get; }
		public int[] Lengths { get; }

		private readonly ValueBufferWriterDelegate<T> _writer;

		public VariableLengthTypeArrayBuffer(T[] values, ValueTypeMarker marker, ValueBufferWriterDelegate<T> writer, Func<T, int> bufferLengthGetter) : base(marker)
		{
			Values = values;
			Lengths = new int[values.Length];

			_writer = writer;
			for (int i = 0; i < Lengths.Length; ++i)
			{
				Lengths[i] = bufferLengthGetter(values[i]);
			}
		}

		public override int GetLength()
		{
			var length = sizeof(int) + sizeof(int) * Lengths.Length;
			foreach (var len in Lengths)
			{
				length += len;
			}

			return length;
		}

		public override object GetValue()
		{
			return Values;
		}

		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteInt32(destination, Values.Length);

			var offsetOffset = sizeof(int);
			var bufferOffset = sizeof(int) + sizeof(int) * Lengths.Length;
			var nextBufferStartOffsetRelative = 0;
			for (var i = 0; i < Values.Length; ++i)
			{
				nextBufferStartOffsetRelative += Lengths[i];

				ValueBufferRawHelpers.WriteInt32(destination[offsetOffset..], nextBufferStartOffsetRelative);
				_writer(destination[bufferOffset..], Values[i]);

				offsetOffset += sizeof(int);
				bufferOffset += Lengths[i];
			}
		}
	}
}
