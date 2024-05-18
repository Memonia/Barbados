using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueBooleanBuffer(bool value) : ValueFixedLengthBuffer<bool>(value, ValueTypeMarker.Boolean)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteBoolean(destination, Value);
		}
	}
}
