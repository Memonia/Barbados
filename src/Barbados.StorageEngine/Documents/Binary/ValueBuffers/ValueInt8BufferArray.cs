using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueInt8BufferArray(sbyte[] values) : ValueFixedLengthBufferArray<sbyte>(values, ValueTypeMarker.Int8)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteInt8(destination, Values[index]);
		}
	}
}
