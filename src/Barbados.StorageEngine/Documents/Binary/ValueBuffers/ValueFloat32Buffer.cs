using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueFloat32Buffer(float value) : ValueFixedLengthBuffer<float>(value, ValueTypeMarker.Float32)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteFloat32(destination, Value);
		}
	}
}
