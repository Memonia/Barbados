using System;
using System.Collections.Generic;

using Barbados.Documents.Serialisation.Metadata;
using Barbados.Documents.Serialisation.Values;

namespace Barbados.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		public static bool TryGetBuffer(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> prefix, out IValueBuffer valueBuffer)
		{
			if (_tryGetValueBufferRaw(buffer, prefix, out var valueBufferRaw, out var descriptor))
			{
				valueBuffer = ValueBufferFactory.CreateFromRawBuffer(valueBufferRaw, descriptor.Marker);
				return true;
			}

			valueBuffer = default!;
			return false;
		}

		public static RadixTreeBuffer ExtractWithPrefix(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> prefix)
		{
			/* Extracting values whose keys start with a given prefix is as simple as copying
			 * a subtree which corresponds to the prefix. The operation is identical to building
			 * the tree from scratch (see 'RadixTreeBuffer.Builder.Static'), except we operate
			 * on offsets in the current buffer rather than in-memory tree nodes.
			 * 
			 * The reason why we don't reuse existing builder is because it requires too many unnecessary 
			 * allocations. There's no need to build an in-memory tree just to serialise it back because
			 * the resulting tree is already serialised in the current buffer
			 */

			var rootOffset = _getNodeOffset(buffer, prefix);
			if (rootOffset < 0)
			{
				return Builder.EmptyBuffer;
			}

			// We use current buffer's node offsets as dictionary keys, which saves us an extra integer in 'NodeInfo'
			var extractedRelativeNodeOffsets = new Dictionary<int, int>();
			var extractedPrefixTableLength = 0;
			var extractedValueTableLength = 0;
			var currentValueTableOffset = _getValueTableOffset(buffer);
			var bfnenr = new BreadthFirstNoRootNodeEnumerator(buffer, rootOffset);
			while (bfnenr.TryGetNext(out var info))
			{
				extractedRelativeNodeOffsets.Add(info.Offset, extractedPrefixTableLength);
				extractedPrefixTableLength += PrefixDescriptor.BinaryLength + info.Descriptor.PrefixLength;
				if (info.Descriptor.HasValue)
				{
					extractedPrefixTableLength += ValueDescriptor.BinaryLength;
					var vdo = info.Offset + PrefixDescriptor.BinaryLength;
					var vd = _getValueDescriptor(buffer, vdo);

					var valueOffset = currentValueTableOffset + vd.RelativeOffset;
					var value = ValueBufferRawHelpers.GetBufferBytes(buffer[valueOffset..], vd.Marker);
					extractedValueTableLength += value.Length;
				}
			}

			var extracted = new byte[PrefixTableOffset + extractedPrefixTableLength + extractedValueTableLength];
			var extractedSpan = extracted.AsSpan();
			var extractedPrefixTableOffset = PrefixTableOffset;
			var extractedValueTableOffset = extractedPrefixTableOffset + extractedPrefixTableLength;
			var extractedRelativeValueTableOffset = 0;
			bfnenr = new BreadthFirstNoRootNodeEnumerator(buffer, rootOffset);

			ValueBufferRawHelpers.WriteInt32(extractedSpan, extractedValueTableOffset);
			while (bfnenr.TryGetNext(out var info))
			{
				var firstChildRelativeOffset = info.Descriptor.FirstChildRelativeOffset;
				if (info.Descriptor.HasChildren)
				{
					var firstChildOffset = PrefixTableOffset + firstChildRelativeOffset;
					firstChildRelativeOffset = extractedRelativeNodeOffsets[firstChildOffset];
				}

				var pd = info.Descriptor with
				{
					FirstChildRelativeOffset = firstChildRelativeOffset
				};

				ValueBufferRawHelpers.WriteUInt32(extractedSpan[extractedPrefixTableOffset..], pd.Bits);
				extractedPrefixTableOffset += PrefixDescriptor.BinaryLength;

				var po = info.Offset + PrefixDescriptor.BinaryLength;
				if (info.Descriptor.HasValue)
				{
					var vdo = info.Offset + PrefixDescriptor.BinaryLength;
					var vd = _getValueDescriptor(buffer, vdo);
					var valueOffset = currentValueTableOffset + vd.RelativeOffset;
					var value = ValueBufferRawHelpers.GetBufferBytes(buffer[valueOffset..], vd.Marker);

					vd = vd with
					{
						RelativeOffset = extractedRelativeValueTableOffset
					};

					ValueBufferRawHelpers.WriteUInt32(extractedSpan[extractedPrefixTableOffset..], vd.Bits);
					extractedPrefixTableOffset += ValueDescriptor.BinaryLength;

					value.CopyTo(extractedSpan[extractedValueTableOffset..]);
					extractedValueTableOffset += value.Length;
					extractedRelativeValueTableOffset += value.Length;

					po += ValueDescriptor.BinaryLength;
				}

				var currentPrefix = buffer.Slice(po, pd.PrefixLength);
				currentPrefix.CopyTo(extractedSpan[extractedPrefixTableOffset..]);
				extractedPrefixTableOffset += pd.PrefixLength;
			}

			return new RadixTreeBuffer(extracted);
		}

		private static bool _isEmpty(ReadOnlySpan<byte> buffer)
		{
			return buffer.Length <= 4;
		}

		private static bool _tryGetValueBufferRaw(
			ReadOnlySpan<byte> buffer,
			ReadOnlySpan<byte> prefix,
			out ReadOnlySpan<byte> valueBuffer,
			out ValueDescriptor descriptor
			)
		{
			if (!_tryGetValueDescriptor(buffer, prefix, out descriptor))
			{
				valueBuffer = default!;
				return false;
			}

			valueBuffer = _getValueBuffer(buffer, descriptor);
			return true;
		}

		private static bool _tryGetValueDescriptor(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> prefix, out ValueDescriptor descriptor)
		{
			var offset = _getNodeOffset(buffer, prefix);
			if (offset < 0)
			{
				descriptor = default!;
				return false;
			}

			var pd = _getPrefixDescriptor(buffer, offset);
			if (!pd.HasValue)
			{
				descriptor = default!;
				return false;
			}

			descriptor = _getValueDescriptor(buffer, offset + PrefixDescriptor.BinaryLength);
			return true;
		}

		private static int _getNodeOffset(ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> prefix)
		{
			if (_isEmpty(buffer))
			{
				return -1;
			}

			int nodeOffset = PrefixTableOffset;
			int prefixSearchPosition = 0;
			while (prefixSearchPosition < prefix.Length)
			{
				var currentSearchPrefix = prefix[prefixSearchPosition..];
				var pd = _getPrefixDescriptor(buffer, nodeOffset);
				var info = new NodeInfo(nodeOffset, pd);

				var currentPrefix = _getNodePrefix(buffer, info);
				var cpl = currentSearchPrefix.CommonPrefixLength(currentPrefix);
				if (cpl == 0)
				{
					if (pd.IsLastChild)
					{
						break;
					}

					nodeOffset = _getNextNodeOffset(buffer, info);
					continue;
				}

				prefixSearchPosition += cpl;
				if (cpl == currentPrefix.Length && cpl == currentSearchPrefix.Length)
				{
					return nodeOffset;
				}

				if (!pd.HasChildren)
				{
					break;
				}

				nodeOffset = _getNodeFirstChildOffset(buffer, pd);
			}

			return -1;
		}

		private static int _getNextNodeOffset(ReadOnlySpan<byte> buffer, NodeInfo info)
		{
			var offset = info.Offset + PrefixDescriptor.BinaryLength;
			if (info.Descriptor.HasValue)
			{
				offset += ValueDescriptor.BinaryLength;
			}

			return offset + info.Descriptor.PrefixLength;
		}

		private static int _getNodeFirstChildOffset(ReadOnlySpan<byte> buffer, PrefixDescriptor descriptor)
		{
			return PrefixTableOffset + descriptor.FirstChildRelativeOffset;
		}

		private static int _getValueTableOffset(ReadOnlySpan<byte> buffer)
		{
			return ValueBufferRawHelpers.ReadInt32(buffer);
		}

		private static ReadOnlySpan<byte> _getNodePrefix(ReadOnlySpan<byte> buffer, NodeInfo info)
		{
			var offset = info.Offset + PrefixDescriptor.BinaryLength;
			if (info.Descriptor.HasValue)
			{
				offset += ValueDescriptor.BinaryLength;
			}

			return buffer.Slice(offset, info.Descriptor.PrefixLength);
		}

		private static ReadOnlySpan<byte> _getValueBuffer(ReadOnlySpan<byte> buffer, ValueDescriptor descriptor)
		{
			var valueTableOffset = ValueBufferRawHelpers.ReadInt32(buffer);
			var valueOffset = valueTableOffset + descriptor.RelativeOffset;
			return ValueBufferRawHelpers.GetBufferBytes(buffer[valueOffset..], descriptor.Marker);
		}

		private static ValueDescriptor _getValueDescriptor(ReadOnlySpan<byte> buffer, int offset)
		{
			return new ValueDescriptor(ValueBufferRawHelpers.ReadUInt32(buffer[offset..]));
		}

		private static PrefixDescriptor _getPrefixDescriptor(ReadOnlySpan<byte> buffer, int offset)
		{
			return new PrefixDescriptor(ValueBufferRawHelpers.ReadUInt32(buffer[offset..]));
		}
	}
}
