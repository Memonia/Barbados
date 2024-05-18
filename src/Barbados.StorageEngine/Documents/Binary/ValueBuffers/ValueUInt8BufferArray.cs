using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueUInt8BufferArray(byte[] values) : ValueFixedLengthBufferArray<byte>(values, ValueTypeMarker.UInt8)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteUInt8(destination, Values[index]);
		}
	}
}
