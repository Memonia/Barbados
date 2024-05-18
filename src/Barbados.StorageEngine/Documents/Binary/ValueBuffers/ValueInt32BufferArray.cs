using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueInt32BufferArray(int[] values) : ValueFixedLengthBufferArray<int>(values, ValueTypeMarker.Int32)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteInt32(destination, Values[index]);
		}
	}
}
