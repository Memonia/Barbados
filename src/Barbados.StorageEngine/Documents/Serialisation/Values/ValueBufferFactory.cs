using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal static partial class ValueBufferFactory
	{
		public static FixedLengthTypeValueBuffer<sbyte> Create(sbyte value)
		{
			return new FixedLengthTypeValueBuffer<sbyte>(
				value, 
				ValueTypeMarker.Int8,
				(destination, value) => ValueBufferRawHelpers.WriteInt8(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<short> Create(short value)
		{
			return new FixedLengthTypeValueBuffer<short>(
				value,
				ValueTypeMarker.Int16,
				(destination, value) => ValueBufferRawHelpers.WriteInt16(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<int> Create(int value)
		{
			return new FixedLengthTypeValueBuffer<int>(
				value,
				ValueTypeMarker.Int32,
				(destination, value) => ValueBufferRawHelpers.WriteInt32(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<long> Create(long value)
		{
			return new FixedLengthTypeValueBuffer<long>(
				value,
				ValueTypeMarker.Int64,
				(destination, value) => ValueBufferRawHelpers.WriteInt64(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<byte> Create(byte value)
		{
			return new FixedLengthTypeValueBuffer<byte>(
				value,
				ValueTypeMarker.UInt8,
				(destination, value) => ValueBufferRawHelpers.WriteUInt8(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<ushort> Create(ushort value)
		{
			return new FixedLengthTypeValueBuffer<ushort>(
				value,
				ValueTypeMarker.UInt16,
				(destination, value) => ValueBufferRawHelpers.WriteUInt16(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<uint> Create(uint value)
		{
			return new FixedLengthTypeValueBuffer<uint>(
				value,
				ValueTypeMarker.UInt32,
				(destination, value) => ValueBufferRawHelpers.WriteUInt32(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<ulong> Create(ulong value)
		{
			return new FixedLengthTypeValueBuffer<ulong>(
				value,
				ValueTypeMarker.UInt64,
				(destination, value) => ValueBufferRawHelpers.WriteUInt64(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<float> Create(float value)
		{
			return new FixedLengthTypeValueBuffer<float>(
				value,
				ValueTypeMarker.Float32,
				(destination, value) => ValueBufferRawHelpers.WriteFloat32(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<double> Create(double value)
		{
			return new FixedLengthTypeValueBuffer<double>(
				value,
				ValueTypeMarker.Float64,
				(destination, value) => ValueBufferRawHelpers.WriteFloat64(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<bool> Create(bool value)
		{
			return new FixedLengthTypeValueBuffer<bool>(
				value,
				ValueTypeMarker.Boolean,
				(destination, value) => ValueBufferRawHelpers.WriteBoolean(destination, value)
			);
		}

		public static FixedLengthTypeValueBuffer<DateTime> Create(DateTime value)
		{
			return new FixedLengthTypeValueBuffer<DateTime>(
				value,
				ValueTypeMarker.DateTime,
				(destination, value) => ValueBufferRawHelpers.WriteDateTime(destination, value)
			);
		}

		public static VariableLengthTypeValueBuffer<string> Create(string value)
		{
			return new VariableLengthTypeValueBuffer<string>(
				value,
				value.Length,
				ValueTypeMarker.String,
				(destination, value) => ValueBufferRawHelpers.WriteStringValue(destination, value)
			);
		}

		public static IValueBuffer CreateFromRawBuffer(ReadOnlySpan<byte> buffer, ValueTypeMarker marker)
		{
			return marker switch
			{
				ValueTypeMarker.Int8 => Create(ValueBufferRawHelpers.ReadInt8(buffer)),
				ValueTypeMarker.Int16 => Create(ValueBufferRawHelpers.ReadInt16(buffer)),
				ValueTypeMarker.Int32 => Create(ValueBufferRawHelpers.ReadInt32(buffer)),
				ValueTypeMarker.Int64 => Create(ValueBufferRawHelpers.ReadInt64(buffer)),
				ValueTypeMarker.UInt8 => Create(ValueBufferRawHelpers.ReadUInt8(buffer)),
				ValueTypeMarker.UInt16 => Create(ValueBufferRawHelpers.ReadUInt16(buffer)),
				ValueTypeMarker.UInt32 => Create(ValueBufferRawHelpers.ReadUInt32(buffer)),
				ValueTypeMarker.UInt64 => Create(ValueBufferRawHelpers.ReadUInt64(buffer)),
				ValueTypeMarker.Float32 => Create(ValueBufferRawHelpers.ReadFloat32(buffer)),
				ValueTypeMarker.Float64 => Create(ValueBufferRawHelpers.ReadFloat64(buffer)),
				ValueTypeMarker.Boolean => Create(ValueBufferRawHelpers.ReadBoolean(buffer)),
				ValueTypeMarker.DateTime => Create(ValueBufferRawHelpers.ReadDateTime(buffer)),
				ValueTypeMarker.String => Create(ValueBufferRawHelpers.ReadStringFromBuffer(buffer)),
				ValueTypeMarker.ArrayInt8 => Create(ValueBufferRawHelpers.ReadInt8Array(buffer)),
				ValueTypeMarker.ArrayInt16 => Create(ValueBufferRawHelpers.ReadInt16Array(buffer)),
				ValueTypeMarker.ArrayInt32 => Create(ValueBufferRawHelpers.ReadInt32Array(buffer)),
				ValueTypeMarker.ArrayInt64 => Create(ValueBufferRawHelpers.ReadInt64Array(buffer)),
				ValueTypeMarker.ArrayUInt8 => Create(ValueBufferRawHelpers.ReadUInt8Array(buffer)),
				ValueTypeMarker.ArrayUInt16 => Create(ValueBufferRawHelpers.ReadUInt16Array(buffer)),
				ValueTypeMarker.ArrayUInt32 => Create(ValueBufferRawHelpers.ReadUInt32Array(buffer)),
				ValueTypeMarker.ArrayUInt64 => Create(ValueBufferRawHelpers.ReadUInt64Array(buffer)),
				ValueTypeMarker.ArrayFloat32 => Create(ValueBufferRawHelpers.ReadFloat32Array(buffer)),
				ValueTypeMarker.ArrayFloat64 => Create(ValueBufferRawHelpers.ReadFloat64Array(buffer)),
				ValueTypeMarker.ArrayDateTime => Create(ValueBufferRawHelpers.ReadDateTimeArray(buffer)),
				ValueTypeMarker.ArrayBoolean => Create(ValueBufferRawHelpers.ReadBooleanArray(buffer)),
				ValueTypeMarker.ArrayString => Create(ValueBufferRawHelpers.ReadStringArray(buffer)),
				_ => throw new NotImplementedException()
			};
		}
	}
}
