using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary.ValueBuffers;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public static ObjectBuffer Collect(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> name)
		{
			var index = _descriptorBinarySearch(buffer, name, prefixComparison: true);
			if (index < 0)
			{
				return Builder.Build(new());
			}

			/* Since all tables are sorted in the names' order, we just have to find the ranges 
			 * of items which belong to the same name prefix and copy them to the new buffer,
			 * not forgetting about adjusting the offsets in the new descriptor table.
			 */

			var count = _count(buffer);
			var comparer = ValueSpanComparerFactory.GetComparer(ValueTypeMarker.String);

			int left = index;
			int right = index;

			// Find where the range starts
			for (; left >= 0; --left)
			{
				var storedName = _getNameBufferValueBytes(buffer, _getDescriptor(buffer, left));
				if (storedName.Length < name.Length || comparer.Compare(name, storedName[..name.Length]) != 0)
				{
					left += 1;
					break;
				}
			}

			if (left < 0)
			{
				left += 1;
			}

			// Find where the range ends
			for (; right < count; ++right)
			{
				var storedName = _getNameBufferValueBytes(buffer, _getDescriptor(buffer, right));
				if (storedName.Length < name.Length || comparer.Compare(name, storedName[..name.Length]) != 0)
				{
					right -= 1;
					break;
				}
			}

			if (right == count)
			{
				right -= 1;
			}

			var vdto = _getValueDescriptorTableOffset(buffer);
			var vto = _getValueTableOffset(buffer);
			var nto = _getNameTableOffset(buffer);
			var leftDescriptor = _getDescriptor(buffer, left);
			var rightDescriptor = _getDescriptor(buffer, right);

			var vdtLeftRelativeOffset = left * ValueDescriptor.BinaryLength;
			var vtLeftRelativeOffset = leftDescriptor.ValueOffset;
			var ntLeftRelativeOffset = leftDescriptor.NameOffset;

			var rightBufferLength = _getValueBufferBytes(buffer, rightDescriptor).Length;
			var rightNameBufferLength = _getNameBufferBytes(buffer, rightDescriptor).Length;

			var vdtLength = (right + 1) * ValueDescriptor.BinaryLength - vdtLeftRelativeOffset;
			var vtLength = rightDescriptor.ValueOffset + rightBufferLength - leftDescriptor.ValueOffset;
			var ntLength = rightDescriptor.NameOffset + rightNameBufferLength - leftDescriptor.NameOffset;

			var vdtSpan = buffer.Slice(vdto + vdtLeftRelativeOffset, vdtLength);
			var vtSpan = buffer.Slice(vto + vtLeftRelativeOffset, vtLength);
			var ntSpan = buffer.Slice(nto + ntLeftRelativeOffset, ntLength);

			var i = 0;
			var nbuf = new byte[sizeof(int) * 3 + vdtLength + vtLength + ntLength];
			var nbufSpan = nbuf.AsSpan();

			ValueBufferRawHelpers.WriteInt32(nbufSpan[i..], vdtLength);
			i += sizeof(int);
			ValueBufferRawHelpers.WriteInt32(nbufSpan[i..], vtLength);
			i += sizeof(int);
			ValueBufferRawHelpers.WriteInt32(nbufSpan[i..], ntLength);
			i += sizeof(int);
			vdtSpan.CopyTo(nbufSpan[i..]);
			i += vdtLength;
			vtSpan.CopyTo(nbufSpan[i..]);
			i += vtLength;
			ntSpan.CopyTo(nbufSpan[i..]);
			i += ntLength;

			Debug.Assert(i == nbuf.Length);

			// Adjust descriptor offsets
			for (i = left; i <= right; ++i)
			{
				var descriptor = _getDescriptor(buffer, i);
				var updated = descriptor with
				{
					NameOffset = descriptor.NameOffset - ntLeftRelativeOffset,
					ValueOffset = descriptor.ValueOffset - vtLeftRelativeOffset
				};

				_setDescriptor(nbuf, i - left, updated);
			}

			return new(nbuf);
		}

		public static ObjectBuffer Select(ReadOnlySpan<byte> buffer, ValueSelector selector)
		{
			var acc = new Builder.Accumulator();
			foreach (var identifier in selector)
			{
				if (acc.Contains(new ValueStringBuffer(identifier)))
				{
					continue;
				}

				if (identifier.IsGroup)
				{
					var collected = Collect(buffer, identifier.StringBufferValue);
					var e = collected.GetNameEnumerator();
					while (e.TryGetNext(out var raw, out var name))
					{
						var nameBuffer = new ValueStringBuffer(name);
						if (acc.Contains(nameBuffer))
						{
							continue;
						}

						var r = collected.TryGetBuffer(raw, out var valueBuffer);
						Debug.Assert(r);

						acc.Add(nameBuffer, valueBuffer);
					}
				}

				else
				{
					var index = _descriptorBinarySearch(buffer, identifier.StringBufferValue, false);
					if (index >= 0)
					{
						var descriptor = _getDescriptor(buffer, index);
						var valueBufferRaw = _getValueBufferBytes(buffer, descriptor);
						acc.Add(
							new ValueStringBuffer(identifier),
							ValueBufferFactory.CreateFromRawBuffer(valueBufferRaw, descriptor.Marker)
						);
					}
				}
			}

			return Builder.Build(acc);
		}

		private static void _setDescriptor(Span<byte> buffer, int index, ValueDescriptor descriptor)
		{
			Debug.Assert(index < _count(buffer));
			var i = _getValueDescriptorTableOffset(buffer) + index * ValueDescriptor.BinaryLength;
			ValueBufferRawHelpers.WriteUInt64(buffer[i..], descriptor.Bits);
		}

		private static ValueDescriptor _getDescriptor(ReadOnlySpan<byte> buffer, int index)
		{
			Debug.Assert(index < _count(buffer));
			var i = _getValueDescriptorTableOffset(buffer) + index * ValueDescriptor.BinaryLength;
			return new(ValueBufferRawHelpers.ReadUInt64(buffer[i..]));
		}

		private static int _getValueDescriptorTableOffset(ReadOnlySpan<byte> buffer)
		{
			return sizeof(int) * 3;
		}

		private static int _getValueDescriptorTableLength(ReadOnlySpan<byte> buffer)
		{
			return ValueBufferRawHelpers.ReadInt32(buffer);
		}

		private static int _getValueTableOffset(ReadOnlySpan<byte> buffer)
		{
			return _getValueDescriptorTableOffset(buffer) + _getValueDescriptorTableLength(buffer);
		}

		private static int _getValueTableLength(ReadOnlySpan<byte> buffer)
		{
			return ValueBufferRawHelpers.ReadInt32(buffer.Slice(sizeof(int), sizeof(int)));
		}

		private static int _getNameTableOffset(ReadOnlySpan<byte> buffer)
		{
			return _getValueTableOffset(buffer) + _getValueTableLength(buffer);
		}

		private static int _getNameTableLength(ReadOnlySpan<byte> buffer)
		{
			return ValueBufferRawHelpers.ReadInt32(buffer.Slice(sizeof(int) * 2, sizeof(int)));
		}

		private static int _count(ReadOnlySpan<byte> buffer)
		{
			return _getValueDescriptorTableLength(buffer) / ValueDescriptor.BinaryLength;
		}

		private static ReadOnlySpan<byte> _getNameBufferBytes(ReadOnlySpan<byte> buffer, ValueDescriptor descriptor)
		{
			var start = _getNameTableOffset(buffer) + descriptor.NameOffset;
			return ValueBufferRawHelpers.GetBufferBytes(buffer[start..], ValueTypeMarker.String);
		}

		private static ReadOnlySpan<byte> _getNameBufferValueBytes(ReadOnlySpan<byte> buffer, ValueDescriptor descriptor)
		{
			var valueBuffer = _getNameBufferBytes(buffer, descriptor);
			return ValueBufferRawHelpers.GetBufferValueBytes(valueBuffer, ValueTypeMarker.String);
		}

		private static ReadOnlySpan<byte> _getValueBufferBytes(ReadOnlySpan<byte> buffer, ValueDescriptor descriptor)
		{
			var start = _getValueTableOffset(buffer) + descriptor.ValueOffset;
			return descriptor.IsArray
				? ValueBufferRawHelpers.GetBufferArrayBytes(buffer[start..], descriptor.Marker)
				: ValueBufferRawHelpers.GetBufferBytes(buffer[start..], descriptor.Marker);
		}

		private static int _descriptorBinarySearch(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> name, bool prefixComparison)
		{
			int left = 0;
			int right = _count(buffer) - 1;
			var comparer = ValueSpanComparerFactory.GetComparer(ValueTypeMarker.String);
			while (left <= right)
			{
				// Idea taken from .NET runtime implementation
				int m = (int)((uint)left + (uint)right >> 1);
				int c;

				var storedName = _getNameBufferValueBytes(buffer, _getDescriptor(buffer, m));
				if (prefixComparison && storedName.Length >= name.Length)
				{
					c = comparer.Compare(name, storedName[..name.Length]);
				}

				else
				{
					c = comparer.Compare(name, storedName);
				}

				if (c == 0)
				{
					return m;
				}

				else
				if (c > 0)
				{
					left = m + 1;
				}

				else
				{
					right = m - 1;
				}
			}

			return ~left;
		}
	}
}
