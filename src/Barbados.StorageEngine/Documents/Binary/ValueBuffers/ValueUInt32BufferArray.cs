using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueUInt32BufferArray(uint[] values) : ValueFixedLengthBufferArray<uint>(values, ValueTypeMarker.UInt32)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteUInt32(destination, Values[index]);
		}
	}
}
