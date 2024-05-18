using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueInt32Buffer(int value) : ValueFixedLengthBuffer<int>(value, ValueTypeMarker.Int32)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteInt32(destination, Value);
		}
	}
}
