using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public bool TryGetInt8(ReadOnlySpan<byte> name, out sbyte value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.Int8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt8(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt16(ReadOnlySpan<byte> name, out short value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.Int16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt16(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt32(ReadOnlySpan<byte> name, out int value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.Int32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt32(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt64(ReadOnlySpan<byte> name, out long value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.Int64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt64(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt8(ReadOnlySpan<byte> name, out byte value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.UInt8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt8(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt16(ReadOnlySpan<byte> name, out ushort value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.UInt16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt16(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt32(ReadOnlySpan<byte> name, out uint value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.UInt32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt32(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt64(ReadOnlySpan<byte> name, out ulong value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.UInt64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt64(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat32(ReadOnlySpan<byte> name, out float value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.Float32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat32(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat64(ReadOnlySpan<byte> name, out double value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.Float64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat64(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetBoolean(ReadOnlySpan<byte> name, out bool value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.Boolean, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadBoolean(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetDateTime(ReadOnlySpan<byte> name, out DateTime value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.DateTime, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadDateTime(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetString(ReadOnlySpan<byte> name, out string value)
		{
			if (TryGetBufferRaw(name, ValueTypeMarker.String, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadStringFromBuffer(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}
	}
}
