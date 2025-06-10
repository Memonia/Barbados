using System;

using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		public bool TryGetInt8Array(RadixTreePrefixSpan prefix, out sbyte[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt8Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt16Array(RadixTreePrefixSpan prefix, out short[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt16Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt32Array(RadixTreePrefixSpan prefix, out int[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt64Array(RadixTreePrefixSpan prefix, out long[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt8Array(RadixTreePrefixSpan prefix, out byte[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt8Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt16Array(RadixTreePrefixSpan prefix, out ushort[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt16Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt32Array(RadixTreePrefixSpan prefix, out uint[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt64Array(RadixTreePrefixSpan prefix, out ulong[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat32Array(RadixTreePrefixSpan prefix, out float[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayFloat32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat64Array(RadixTreePrefixSpan prefix, out double[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayFloat64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetBooleanArray(RadixTreePrefixSpan prefix, out bool[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayBoolean, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadBooleanArray(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetDateTimeArray(RadixTreePrefixSpan prefix, out DateTime[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayDateTime, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadDateTimeArray(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetStringArray(RadixTreePrefixSpan prefix, out string[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayString, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadStringArray(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}
	}
}
