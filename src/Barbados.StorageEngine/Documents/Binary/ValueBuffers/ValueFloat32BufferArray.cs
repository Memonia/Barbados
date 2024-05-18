using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueFloat32BufferArray(float[] values) : ValueFixedLengthBufferArray<float>(values, ValueTypeMarker.Float32)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteFloat32(destination, Values[index]);
		}
	}
}
