using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueFloat64Buffer(double value) : ValueFixedLengthBuffer<double>(value, ValueTypeMarker.Float64)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteFloat64(destination, Value);
		}
	}
}
