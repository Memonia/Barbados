using System;
using System.Diagnostics;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal abstract class ValueVariableLengthBufferArray<T> : ValueBuffer
	{
		/* Format:
		 *   I32 count,
		 *   I32[count] offsets relative to the start of the first buffer,
		 *   ValueBuffer[count] buffers
		 *   
		 *  Reason: 
		 *   Enables constant-time random access for elements of the array
		 */

		public T[] Values { get; }
		public int[] Lengths { get; }

		public ValueVariableLengthBufferArray(T[] values, int[] lengths, ValueTypeMarker marker) : base(marker, true)
		{
			Debug.Assert(values.Length == lengths.Length);

			Values = values;
			Lengths = lengths;
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
				WriteValueTo(destination[bufferOffset..], i);

				offsetOffset += sizeof(int);
				bufferOffset += Lengths[i];
			}
		}

		public abstract void WriteValueTo(Span<byte> destination, int index);
	}
}
