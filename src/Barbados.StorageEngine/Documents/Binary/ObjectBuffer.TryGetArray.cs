using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public bool TryGetInt8Array(ReadOnlySpan<byte> name, out sbyte[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.Int8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt8Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt16Array(ReadOnlySpan<byte> name, out short[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.Int16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt16Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt32Array(ReadOnlySpan<byte> name, out int[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.Int32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetInt64Array(ReadOnlySpan<byte> name, out long[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.Int64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadInt64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt8Array(ReadOnlySpan<byte> name, out byte[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.UInt8, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt8Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt16Array(ReadOnlySpan<byte> name, out ushort[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.UInt16, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt16Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt32Array(ReadOnlySpan<byte> name, out uint[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.UInt32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetUInt64Array(ReadOnlySpan<byte> name, out ulong[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.UInt64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadUInt64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat32Array(ReadOnlySpan<byte> name, out float[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.Float32, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat32Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetFloat64Array(ReadOnlySpan<byte> name, out double[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.Float64, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadFloat64Array(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetBooleanArray(ReadOnlySpan<byte> name, out bool[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.Boolean, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadBooleanArray(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetDateTimeArray(ReadOnlySpan<byte> name, out DateTime[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.DateTime, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadDateTimeArray(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryGetStringArray(ReadOnlySpan<byte> name, out string[] value)
		{
			if (TryGetBufferArrayRaw(name, ValueTypeMarker.String, out var rawBuffer))
			{
				value = ValueBufferRawHelpers.ReadStringArray(rawBuffer);
				return true;
			}

			value = default!;
			return false;
		}
	}
}
