using System;

using Barbados.Documents.Serialisation.Values;

namespace Barbados.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		public bool TryGetInt8Array(ReadOnlySpan<byte> prefix, out sbyte[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt8Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt16Array(ReadOnlySpan<byte> prefix, out short[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt16Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt32Array(ReadOnlySpan<byte> prefix, out int[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt64Array(ReadOnlySpan<byte> prefix, out long[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt8Array(ReadOnlySpan<byte> prefix, out byte[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt8Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt16Array(ReadOnlySpan<byte> prefix, out ushort[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt16Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt32Array(ReadOnlySpan<byte> prefix, out uint[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt64Array(ReadOnlySpan<byte> prefix, out ulong[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat32Array(ReadOnlySpan<byte> prefix, out float[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayFloat32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat64Array(ReadOnlySpan<byte> prefix, out double[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayFloat64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetBooleanArray(ReadOnlySpan<byte> prefix, out bool[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayBoolean, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadBooleanArray(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetDateTimeArray(ReadOnlySpan<byte> prefix, out DateTime[] value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayDateTime, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadDateTimeArray(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetStringArray(ReadOnlySpan<byte> prefix, out string[] value)
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
