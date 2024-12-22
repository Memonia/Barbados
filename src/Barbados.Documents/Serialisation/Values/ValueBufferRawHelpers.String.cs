using System;
using System.Text;

namespace Barbados.Documents.Serialisation.Values
{
	internal partial class ValueBufferRawHelpers
	{
		public static int GetLength(string value)
		{
			return Encoding.UTF8.GetByteCount(value);
		}

		public static void WriteStringValue(Span<byte> buffer, string value)
		{
			Encoding.UTF8.GetBytes(value, buffer);
		}

		public static string ReadStringFromBuffer(ReadOnlySpan<byte> buffer)
		{
			return Encoding.UTF8.GetString(buffer.Slice(sizeof(int), ReadInt32(buffer)));
		}

		public static string[] ReadStringArray(ReadOnlySpan<byte> buffer)
		{
			var arr = new string[GetArrayBufferCount(buffer)];
			for (int i = 0; i < arr.Length; ++i)
			{
				var str = GetStringBytesFromArray(buffer, i);
				arr[i] = Encoding.UTF8.GetString(str);
			}

			return arr;
		}

		public static string ReadStringFromArray(ReadOnlySpan<byte> buffer, int index)
		{
			var str = GetStringBytesFromArray(buffer, index);
			return Encoding.UTF8.GetString(str);
		}

		public static ReadOnlySpan<byte> GetStringBytesFromArray(ReadOnlySpan<byte> buffer, int index)
		{
			var startOffsets = sizeof(int);
			var startBuffers = sizeof(int) + sizeof(int) * GetArrayBufferCount(buffer);
			if (index == 0)
			{
				var length = ReadInt32(buffer[startOffsets..]);
				return buffer.Slice(startBuffers, length);
			}

			else
			{
				var relativeOffset = ReadInt32(buffer[(startOffsets + sizeof(int) * (index - 1))..]);
				var nextOffset = ReadInt32(buffer[(startOffsets + sizeof(int) * index)..]);
				var length = nextOffset - relativeOffset;
				return buffer.Slice(startBuffers + relativeOffset, length);
			}
		}
	}
}
