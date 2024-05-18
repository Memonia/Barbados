using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueBooleanBufferArray(bool[] values) : ValueFixedLengthBufferArray<bool>(values, ValueTypeMarker.Boolean)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteBoolean(destination, Values[index]);
		}
	}
}
