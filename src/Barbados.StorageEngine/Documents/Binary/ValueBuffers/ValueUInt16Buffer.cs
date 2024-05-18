using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueUInt16Buffer(ushort value) : ValueFixedLengthBuffer<ushort>(value, ValueTypeMarker.UInt16)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteUInt16(destination, Value);
		}
	}
}
