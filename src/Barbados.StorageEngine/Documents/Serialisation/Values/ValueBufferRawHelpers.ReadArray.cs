using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal partial class ValueBufferRawHelpers
	{
		private static T[] _readFixedLengthTypeArray<T>(ReadOnlySpan<byte> buffer, int valueLength, ValueBufferReaderDelegate<T> reader)
		{
			var e = new ValueBufferEnumerator(buffer, valueLength);
			var arr = new T[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = reader(valueBuffer);
			}

			return arr;
		}

		public static sbyte[] ReadInt8Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(sbyte), ReadInt8);
		public static short[] ReadInt16Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(short), ReadInt16);
		public static int[] ReadInt32Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(int), ReadInt32);
		public static long[] ReadInt64Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(long), ReadInt64);
		public static byte[] ReadUInt8Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(byte), ReadUInt8);
		public static ushort[] ReadUInt16Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(ushort), ReadUInt16);
		public static uint[] ReadUInt32Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(uint), ReadUInt32);
		public static ulong[] ReadUInt64Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(ulong), ReadUInt64);
		public static float[] ReadFloat32Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(float), ReadFloat32);
		public static double[] ReadFloat64Array(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(double), ReadFloat64);
		public static DateTime[] ReadDateTimeArray(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(long), ReadDateTime);
		public static bool[] ReadBooleanArray(ReadOnlySpan<byte> buffer) => _readFixedLengthTypeArray(buffer, sizeof(byte), ReadBoolean);
	}
}
