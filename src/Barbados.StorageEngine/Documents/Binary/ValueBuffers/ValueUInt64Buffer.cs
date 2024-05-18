using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueUInt64Buffer(ulong value) : ValueFixedLengthBuffer<ulong>(value, ValueTypeMarker.UInt64)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteUInt64(destination, Value);
		}
	}
}
