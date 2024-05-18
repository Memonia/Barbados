using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueDateTimeBuffer(DateTime value) : ValueFixedLengthBuffer<DateTime>(value, ValueTypeMarker.DateTime)
	{
		public override void WriteTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteDateTime(destination, Value);
		}
	}
}
