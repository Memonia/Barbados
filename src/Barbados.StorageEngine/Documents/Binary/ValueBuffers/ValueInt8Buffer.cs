using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueInt8Buffer(sbyte value) : ValueFixedLengthBuffer<sbyte>(value, ValueTypeMarker.Int8)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteInt8(destination, Value);
		}
	}
}
