using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueUInt8Buffer(byte value) : ValueFixedLengthBuffer<byte>(value, ValueTypeMarker.UInt8)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteUInt8(destination, Value);
		}
	}
}
