using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueUInt32Buffer(uint value) : ValueFixedLengthBuffer<uint>(value, ValueTypeMarker.UInt32)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteUInt32(destination, Value);
		}
	}
}
