using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueFloat64BufferArray(double[] values) : ValueFixedLengthBufferArray<double>(values, ValueTypeMarker.Float64)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteFloat64(destination, Values[index]);
		}
	}
}
