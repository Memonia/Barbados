using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueDateTimeBufferArray(DateTime[] values) : ValueFixedLengthBufferArray<DateTime>(values, ValueTypeMarker.DateTime)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteDateTime(destination, Values[index]);
		}
	}
}
