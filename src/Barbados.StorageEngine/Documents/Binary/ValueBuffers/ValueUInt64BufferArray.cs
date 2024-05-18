using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueUInt64BufferArray(ulong[] values) : ValueFixedLengthBufferArray<ulong>(values, ValueTypeMarker.UInt64)
	{
		protected override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteUInt64(destination, Values[index]);
		}
	}
}
