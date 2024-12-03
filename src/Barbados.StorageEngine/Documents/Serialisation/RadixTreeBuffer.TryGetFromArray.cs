using System;

using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		public bool TryGetFromInt8Array(ReadOnlySpan<byte> prefix, int index, out sbyte value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt8, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadInt8FromArray(rawBuffer, index);
					return true;
				}
			}

			value = default!;
			return false;
		}

		public bool TryGetFromInt16Array(ReadOnlySpan<byte> prefix, int index, out short value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt16, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadInt16FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromInt32Array(ReadOnlySpan<byte> prefix, int index, out int value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt32, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadInt32FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromInt64Array(ReadOnlySpan<byte> prefix, int index, out long value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayInt64, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadInt64FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromUInt8Array(ReadOnlySpan<byte> prefix, int index, out byte value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt8, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadUInt8FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromUInt16Array(ReadOnlySpan<byte> prefix, int index, out ushort value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt16, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadUInt16FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromUInt32Array(ReadOnlySpan<byte> prefix, int index, out uint value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt32, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadUInt32FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromUInt64Array(ReadOnlySpan<byte> prefix, int index, out ulong value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayUInt64, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadUInt64FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromFloat32Array(ReadOnlySpan<byte> prefix, int index, out float value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayFloat32, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadFloat32FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromFloat64Array(ReadOnlySpan<byte> prefix, int index, out double value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayFloat64, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadFloat64FromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromDateTimeArray(ReadOnlySpan<byte> prefix, int index, out DateTime value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayDateTime, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadDateTimeFromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromBooleanArray(ReadOnlySpan<byte> prefix, int index, out bool value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayBoolean, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadBooleanFromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}

		public bool TryGetFromStringArray(ReadOnlySpan<byte> prefix, int index, out string value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.ArrayString, out var rawBuffer))
			{
				if (index < ValueBufferRawHelpers.GetArrayBufferCount(rawBuffer))
				{
					value = ValueBufferRawHelpers.ReadStringFromArray(rawBuffer, index);
					return true;
				}
			}
			value = default!;
			return false;
		}
	}
}
