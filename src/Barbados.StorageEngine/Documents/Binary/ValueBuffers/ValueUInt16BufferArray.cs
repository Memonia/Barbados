using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueUInt16BufferArray(ushort[] values) : ValueFixedLengthBufferArray<ushort>(values, ValueTypeMarker.UInt16)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteUInt16(destination, Values[index]);
		}
	}
}
