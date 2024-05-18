using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ValueBufferRawHelpers
	{
		public static sbyte[] ReadInt8Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(sbyte));
			var arr = new sbyte[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadInt8(valueBuffer);
			}

			return arr;
		}

		public static short[] ReadInt16Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(short));
			var arr = new short[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadInt16(valueBuffer);
			}

			return arr;
		}

		public static int[] ReadInt32Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(int));
			var arr = new int[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadInt32(valueBuffer);
			}

			return arr;
		}

		public static long[] ReadInt64Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(long));
			var arr = new long[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadInt64(valueBuffer);
			}

			return arr;
		}

		public static byte[] ReadUInt8Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(byte));
			var arr = new byte[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadUInt8(valueBuffer);
			}

			return arr;
		}

		public static ushort[] ReadUInt16Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(ushort));
			var arr = new ushort[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadUInt16(valueBuffer);
			}

			return arr;
		}

		public static uint[] ReadUInt32Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(uint));
			var arr = new uint[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadUInt32(valueBuffer);
			}

			return arr;
		}

		public static ulong[] ReadUInt64Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(ulong));
			var arr = new ulong[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadUInt64(valueBuffer);
			}

			return arr;
		}

		public static float[] ReadFloat32Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(float));
			var arr = new float[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadFloat32(valueBuffer);
			}

			return arr;
		}

		public static double[] ReadFloat64Array(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(double));
			var arr = new double[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadFloat64(valueBuffer);
			}

			return arr;
		}

		public static DateTime[] ReadDateTimeArray(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(long));
			var arr = new DateTime[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadDateTime(valueBuffer);
			}

			return arr;
		}

		public static bool[] ReadBooleanArray(ReadOnlySpan<byte> buffer)
		{
			var e = new ValueBufferEnumerator(buffer, sizeof(bool));
			var arr = new bool[e.Count];
			while (e.TryGetNext(out var valueBuffer))
			{
				arr[e.CurrentIndex] = ReadBoolean(valueBuffer);
			}

			return arr;
		}

		public static string[] ReadStringArray(ReadOnlySpan<byte> buffer)
		{
			var arr = new string[GetBufferArrayCount(buffer)];
			var offsetOffset = sizeof(int);
			var bufferStartOffset = sizeof(int) + sizeof(int) * arr.Length;
			var previousBufferOffsetRelative = 0;
			for (int i = 0; i < arr.Length; ++i)
			{
				var nextBufferStartOffsetRelative = ReadInt32(buffer[offsetOffset..]);
				offsetOffset += sizeof(int);

				var bufferLength = nextBufferStartOffsetRelative - previousBufferOffsetRelative;
				arr[i] = ReadStringFromValue(
					buffer.Slice(bufferStartOffset, bufferLength)
				);

				previousBufferOffsetRelative = nextBufferStartOffsetRelative;
				bufferStartOffset += bufferLength;
			}

			return arr;
		}
	}
}
