using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueInt64BufferArray(long[] values) : ValueFixedLengthBufferArray<long>(values, ValueTypeMarker.Int64)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteInt64(destination, Values[index]);
		}
	}
}
