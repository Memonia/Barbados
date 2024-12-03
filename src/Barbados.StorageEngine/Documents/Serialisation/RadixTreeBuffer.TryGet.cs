using System;

using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		public bool TryGetInt8(ReadOnlySpan<byte> prefix, out sbyte value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.Int8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt8(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt16(ReadOnlySpan<byte> prefix, out short value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.Int16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt16(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt32(ReadOnlySpan<byte> prefix, out int value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.Int32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt32(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt64(ReadOnlySpan<byte> prefix, out long value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.Int64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt64(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt8(ReadOnlySpan<byte> prefix, out byte value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.UInt8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt8(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt16(ReadOnlySpan<byte> prefix, out ushort value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.UInt16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt16(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt32(ReadOnlySpan<byte> prefix, out uint value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.UInt32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt32(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt64(ReadOnlySpan<byte> prefix, out ulong value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.UInt64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt64(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat32(ReadOnlySpan<byte> prefix, out float value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.Float32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat32(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat64(ReadOnlySpan<byte> prefix, out double value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.Float64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat64(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetBoolean(ReadOnlySpan<byte> prefix, out bool value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.Boolean, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadBoolean(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetDateTime(ReadOnlySpan<byte> prefix, out DateTime value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.DateTime, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadDateTime(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetString(ReadOnlySpan<byte> prefix, out string value)
		{
			if (TryGetBufferRaw(prefix, ValueTypeMarker.String, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadStringFromBuffer(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}
	}
}
