using System;

namespace Barbados.Documents.RadixTree.Values
{
	internal partial class ValueBufferFactory
	{
		public static FixedLengthTypeArrayBuffer<sbyte> Create(sbyte[] array)
		{
			return new FixedLengthTypeArrayBuffer<sbyte>(
				array,
				ValueTypeMarker.ArrayInt8,
				(destination, value) => ValueBufferRawHelpers.WriteInt8(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<short> Create(short[] array)
		{
			return new FixedLengthTypeArrayBuffer<short>(
				array,
				ValueTypeMarker.ArrayInt16,
				(destination, value) => ValueBufferRawHelpers.WriteInt16(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<int> Create(int[] array)
		{
			return new FixedLengthTypeArrayBuffer<int>(
				array,
				ValueTypeMarker.ArrayInt32,
				(destination, value) => ValueBufferRawHelpers.WriteInt32(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<long> Create(long[] array)
		{
			return new FixedLengthTypeArrayBuffer<long>(
				array,
				ValueTypeMarker.ArrayInt64,
				(destination, value) => ValueBufferRawHelpers.WriteInt64(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<byte> Create(byte[] array)
		{
			return new FixedLengthTypeArrayBuffer<byte>(
				array,
				ValueTypeMarker.ArrayUInt8,
				(destination, value) => ValueBufferRawHelpers.WriteUInt8(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<ushort> Create(ushort[] array)
		{
			return new FixedLengthTypeArrayBuffer<ushort>(
				array,
				ValueTypeMarker.ArrayUInt16,
				(destination, value) => ValueBufferRawHelpers.WriteUInt16(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<uint> Create(uint[] array)
		{
			return new FixedLengthTypeArrayBuffer<uint>(
				array,
				ValueTypeMarker.ArrayUInt32,
				(destination, value) => ValueBufferRawHelpers.WriteUInt32(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<ulong> Create(ulong[] array)
		{
			return new FixedLengthTypeArrayBuffer<ulong>(
				array,
				ValueTypeMarker.ArrayUInt64,
				(destination, value) => ValueBufferRawHelpers.WriteUInt64(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<float> Create(float[] array)
		{
			return new FixedLengthTypeArrayBuffer<float>(
				array,
				ValueTypeMarker.ArrayFloat32,
				(destination, value) => ValueBufferRawHelpers.WriteFloat32(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<double> Create(double[] array)
		{
			return new FixedLengthTypeArrayBuffer<double>(
				array,
				ValueTypeMarker.ArrayFloat64,
				(destination, value) => ValueBufferRawHelpers.WriteFloat64(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<DateTime> Create(DateTime[] array)
		{
			return new FixedLengthTypeArrayBuffer<DateTime>(
				array,
				ValueTypeMarker.ArrayDateTime,
				(destination, value) => ValueBufferRawHelpers.WriteDateTime(destination, value)
			);
		}

		public static FixedLengthTypeArrayBuffer<bool> Create(bool[] array)
		{
			return new FixedLengthTypeArrayBuffer<bool>(
				array,
				ValueTypeMarker.ArrayBoolean,
				(destination, value) => ValueBufferRawHelpers.WriteBoolean(destination, value)
			);
		}

		public static VariableLengthTypeArrayBuffer<string> Create(string[] array)
		{	
			return new VariableLengthTypeArrayBuffer<string>(
				array,
				ValueTypeMarker.ArrayString,
				(destination, value) => ValueBufferRawHelpers.WriteStringValue(destination, value),
				ValueBufferRawHelpers.GetLength
			);
		}
	}
}
