using System;
using System.Diagnostics;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public partial class Builder
		{
			public static ObjectBuffer Build(Accumulator accumulator)
			{
				var buffer = _getBufferWithHeader(accumulator);
				var bufferSpan = buffer.AsSpan();
				var absValueTableOffset = _getValueTableOffset(buffer);
				var absNameTableOffset = _getNameTableOffset(buffer);
				var nextValueOffset = absValueTableOffset;
				var nextNameOffset = absNameTableOffset;
				int count = 0;
				foreach (var valueName in accumulator.GetSortedValueNameEnumerator())
				{
					var nameBuffer = valueName.GetBuffer();
					var r = accumulator.TryGet(valueName, out var valueBuffer);
					Debug.Assert(r);

					var descriptor = new ValueDescriptor(
						valueOffset: nextValueOffset - absValueTableOffset,
						nameOffset: nextNameOffset - absNameTableOffset,
						valueBuffer.Marker,
						valueBuffer.IsArray
					);

					valueBuffer.WriteTo(bufferSpan[nextValueOffset..]);
					nameBuffer.WriteTo(bufferSpan[nextNameOffset..]);
					_writeDescriptor(bufferSpan, count, descriptor);

					nextValueOffset += valueBuffer.GetLength();
					nextNameOffset += nameBuffer.GetLength();
					count += 1;
				}

				return new ObjectBuffer(buffer);
			}

			private static byte[] _getBufferWithHeader(Accumulator accumulator)
			{
				var buffer = new byte[
					sizeof(int) * 3 +
					accumulator.DescriptorTableLength +
					accumulator.ValueTableLength +
					accumulator.NameTableLength
				];

				_writeHeader(buffer, accumulator);
				return buffer;
			}

			private static void _writeHeader(Span<byte> buffer, Accumulator accumulator)
			{
				ValueBufferRawHelpers.WriteInt32(buffer, accumulator.DescriptorTableLength);
				ValueBufferRawHelpers.WriteInt32(buffer[sizeof(int)..], accumulator.ValueTableLength);
				ValueBufferRawHelpers.WriteInt32(buffer[(sizeof(int) * 2)..], accumulator.NameTableLength);
			}

			private static void _writeDescriptor(Span<byte> buffer, int index, ValueDescriptor descriptor)
			{
				var offset = _getValueDescriptorTableOffset(buffer);
				offset += index * ValueDescriptor.BinaryLength;
				ValueBufferRawHelpers.WriteUInt64(buffer[offset..], descriptor.Bits);
			}
		}
	}
}
