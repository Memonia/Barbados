using System;

namespace Barbados.Documents.RadixTree.Values
{
	internal partial class ValueBufferRawHelpers
	{
		private static T _readFromFixedLengthTypeArray<T>(ReadOnlySpan<byte> buffer, int valueLength, int index, ValueBufferReaderDelegate<T> reader)
		{
			return reader(buffer[(sizeof(int) + valueLength * index)..]);
		}

		public static sbyte ReadInt8FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(sbyte), index, ReadInt8);
		public static short ReadInt16FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(short), index, ReadInt16);
		public static int ReadInt32FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(int), index, ReadInt32);
		public static long ReadInt64FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(long), index, ReadInt64);
		public static byte ReadUInt8FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(byte), index, ReadUInt8);
		public static ushort ReadUInt16FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(ushort), index, ReadUInt16);
		public static uint ReadUInt32FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(uint), index, ReadUInt32);
		public static ulong ReadUInt64FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(ulong), index, ReadUInt64);
		public static float ReadFloat32FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(float), index, ReadFloat32);
		public static double ReadFloat64FromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(double), index, ReadFloat64);
		public static DateTime ReadDateTimeFromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(long), index, ReadDateTime);
		public static bool ReadBooleanFromArray(ReadOnlySpan<byte> buffer, int index) => _readFromFixedLengthTypeArray(buffer, sizeof(byte), index, ReadBoolean);
	}
}
