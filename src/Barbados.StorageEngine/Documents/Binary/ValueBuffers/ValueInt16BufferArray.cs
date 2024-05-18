using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueInt16BufferArray(short[] values) : ValueFixedLengthBufferArray<short>(values, ValueTypeMarker.Int16)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteInt16(destination, Values[index]);
		}
	}
}
