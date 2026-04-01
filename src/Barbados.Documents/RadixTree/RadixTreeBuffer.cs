using System;

using Barbados.Documents.RadixTree.Metadata;
using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents.RadixTree
{
	internal sealed partial class RadixTreeBuffer
	{
		// sizeof(offset of the value table)
		public const int PrefixTableOffset = sizeof(uint);

		// See 'ValueDescriptor'
		public const int MaxValueTableLength = 1 << 24 - 1;
		// See 'PrefixDescriptor'
		public const int MaxNodePrefixLength = 1 << 6 - 1;
		public const int MaxPrefixTableLength = 1 << 24 - 1;

		// I32 values are used because we cannot allocate a 2GB+ buffer anyway
		/* Buffer structure:
		 * 
		 * I32: offset of the value table
		 * VAR: prefix table
		 * [   
		 *   U32: prefix descriptor (see 'PrefixDescriptor')
		 *	 if (PrefixDescriptor.HasValue == true):
		 *     U32: value descriptor (see 'ValueDescriptor')
		 *   VAR: prefix
		 * ]
		 * 
		 * VAR: value table
		 */

		public int Length => _buffer.Length;

		private readonly byte[] _buffer;

		public RadixTreeBuffer(byte[] buffer)
		{
			_buffer = buffer;
		}

		public bool PrefixExists(RadixTreePrefixSpan prefix)
		{
			return _getNodeOffset(_buffer, prefix.AsBytes()) >= PrefixTableOffset;
		}

		public bool ValueExists(RadixTreePrefixSpan prefix)
		{
			return ValueExists(prefix, out _);
		}

		public bool ValueExists(RadixTreePrefixSpan prefix, out ValueTypeMarker marker)
		{
			var offset = _getNodeOffset(_buffer, prefix.AsBytes());
			if (offset >= 0)
			{
				var pd = _getPrefixDescriptor(_buffer, offset);
				if (pd.HasValue)
				{
					var vd = _getValueDescriptor(_buffer, offset + PrefixDescriptor.BinaryLength);
					marker = vd.Marker;
					return true;
				}
			}

			marker = default!;
			return false;
		}

		public int Count()
		{
			var bfnenr = new PrivateEnums.BreadthFirstNodesNoRoot(_buffer);
			var count = 0;
			while (bfnenr.TryGetNext(out var info))
			{
				if (info.Descriptor.HasValue)
				{
					count += 1;
				}
			}

			return count;
		}

		public bool TryGetArrayBufferItemCount(RadixTreePrefixSpan prefix, out int count)
		{
			if (TryGetBufferRaw(prefix, out var marker, out var buffer) && marker.IsArray())
			{
				count = ValueBufferRawHelpers.GetArrayBufferCount(buffer);
				return true;
			}

			count = -1;
			return false;
		}

		public bool TryExtract(RadixTreePrefixSpan prefix, out RadixTreeBuffer buffer)
		{
			if (IsEmpty(_buffer))
			{
				buffer = default!;
				return false; 
			}

			buffer = ExtractWithPrefix(_buffer, prefix);
			return !IsEmpty(buffer._buffer);
		}

		public bool TryGetBuffer(RadixTreePrefixSpan prefix, out IValueBuffer buffer)
		{
			return TryGetBuffer(_buffer, prefix, out buffer);
		}

		public bool TryGetBufferRaw(RadixTreePrefixSpan prefix, ValueTypeMarker marker, out ReadOnlySpan<byte> buffer)
		{
			return TryGetBufferRaw(prefix, out var m, out buffer) && m == marker;
		}

		public bool TryGetBufferRaw(RadixTreePrefixSpan prefix, out ValueTypeMarker marker, out ReadOnlySpan<byte> buffer)
		{
			if (_tryGetValueBufferRaw(_buffer, prefix.AsBytes(), out var span, out var descriptor))
			{
				marker = descriptor.Marker;
				buffer = span;
				return true;
			}

			marker = default!;
			buffer = default!;
			return false;
		}

		public ReadOnlySpan<byte> AsSpan() => _buffer;
	}
}
