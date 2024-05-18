using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueInt64Buffer(long value) : ValueFixedLengthBuffer<long>(value, ValueTypeMarker.Int64)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteInt64(destination, Value);
		}
	}
}
