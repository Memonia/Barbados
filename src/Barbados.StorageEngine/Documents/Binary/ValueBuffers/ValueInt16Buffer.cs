using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueInt16Buffer(short value) : ValueFixedLengthBuffer<short>(value, ValueTypeMarker.Int16)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteInt16(destination, Value);
		}
	}
}
