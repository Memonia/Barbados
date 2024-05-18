using System;
using System.Buffers.Binary;
using System.Text;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal static partial class ValueBufferRawHelpers
	{
		public static sbyte ReadInt8(ReadOnlySpan<byte> buffer) => (sbyte)buffer[0];
		public static short ReadInt16(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt16LittleEndian(buffer);
		public static int ReadInt32(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt32LittleEndian(buffer);
		public static long ReadInt64(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt64LittleEndian(buffer);
		public static byte ReadUInt8(ReadOnlySpan<byte> buffer) => buffer[0];
		public static ushort ReadUInt16(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt16LittleEndian(buffer);
		public static uint ReadUInt32(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt32LittleEndian(buffer);
		public static ulong ReadUInt64(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt64LittleEndian(buffer);
		public static float ReadFloat32(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadSingleLittleEndian(buffer);
		public static double ReadFloat64(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadDoubleLittleEndian(buffer);
		public static DateTime ReadDateTime(ReadOnlySpan<byte> buffer) => new(ReadInt64(buffer));
		public static bool ReadBoolean(ReadOnlySpan<byte> buffer) => buffer[0] == 1;

		public static void WriteInt8(Span<byte> buffer, sbyte value) => buffer[0] = (byte)value;
		public static void WriteInt16(Span<byte> buffer, short value) => BinaryPrimitives.WriteInt16LittleEndian(buffer, value);
		public static void WriteInt32(Span<byte> buffer, int value) => BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
		public static void WriteInt64(Span<byte> buffer, long value) => BinaryPrimitives.WriteInt64LittleEndian(buffer, value);
		public static void WriteUInt8(Span<byte> buffer, byte value) => buffer[0] = value;
		public static void WriteUInt16(Span<byte> buffer, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);
		public static void WriteUInt32(Span<byte> buffer, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);
		public static void WriteUInt64(Span<byte> buffer, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);
		public static void WriteFloat32(Span<byte> buffer, float value) => BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
		public static void WriteFloat64(Span<byte> buffer, double value) => BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
		public static void WriteDateTime(Span<byte> buffer, DateTime value) => WriteInt64(buffer, value.Ticks);
		public static void WriteBoolean(Span<byte> buffer, bool value) => buffer[0] = value ? (byte)1 : (byte)0;

		public static string ReadStringFromValue(ReadOnlySpan<byte> buffer) => Encoding.UTF8.GetString(buffer);
		public static string ReadStringFromBuffer(ReadOnlySpan<byte> buffer) => Encoding.UTF8.GetString(buffer.Slice(sizeof(int), ReadInt32(buffer)));

		public static void WriteStringValue(Span<byte> buffer, string value) => Encoding.UTF8.GetBytes(value, buffer);

		public static int GetLength(string value)
		{
			return Encoding.UTF8.GetByteCount(value);
		}

		public static int GetBufferArrayCount(ReadOnlySpan<byte> buffer)
		{
			return ReadInt32(buffer);
		}

		public static ReadOnlySpan<byte> GetBufferBytes(ReadOnlySpan<byte> source, ValueTypeMarker marker)
		{
			return marker switch
			{
				ValueTypeMarker.Int8 or 
				ValueTypeMarker.UInt8 or 
				ValueTypeMarker.Boolean
					=> source[..sizeof(byte)],
				ValueTypeMarker.Int16 or
				ValueTypeMarker.UInt16
					=> source[..sizeof(ushort)],
				ValueTypeMarker.Int32 or 
				ValueTypeMarker.UInt32 or
				ValueTypeMarker.Float32
					=> source[..sizeof(uint)],
				ValueTypeMarker.Int64 or
				ValueTypeMarker.UInt64 or
				ValueTypeMarker.Float64 or 
				ValueTypeMarker.DateTime
					=> source[..sizeof(ulong)],
				ValueTypeMarker.String
					=> source[..(sizeof(int) + ReadInt32(source))],
				_ => throw new NotImplementedException()
			};
		}

		public static ReadOnlySpan<byte> GetBufferValueBytes(ReadOnlySpan<byte> buffer, ValueTypeMarker marker)
		{
			return marker switch
			{
				ValueTypeMarker.Int8 or
				ValueTypeMarker.Int16 or 
				ValueTypeMarker.Int32 or
				ValueTypeMarker.Int64 or
				ValueTypeMarker.UInt8 or 
				ValueTypeMarker.UInt16 or
				ValueTypeMarker.UInt32 or 
				ValueTypeMarker.UInt64 or
				ValueTypeMarker.Float32 or
				ValueTypeMarker.Float64 or
				ValueTypeMarker.DateTime or
				ValueTypeMarker.Boolean
					=> buffer,
				ValueTypeMarker.String
					=> buffer[sizeof(int)..],
				_ => throw new NotImplementedException()
			};
		}

		public static ReadOnlySpan<byte> GetBufferArrayBytes(ReadOnlySpan<byte> buffer, ValueTypeMarker marker)
		{
			var count = GetBufferArrayCount(buffer);
			switch (marker)
			{
				case ValueTypeMarker.String:
					var buffersLength = ReadInt32(buffer[(sizeof(int) + sizeof(int) * (count - 1))..]);
					return buffer[..(sizeof(int) + count * sizeof(int) + buffersLength)];

				default:
					return marker switch
					{
						ValueTypeMarker.Int8 => buffer[..(sizeof(int) + count * sizeof(sbyte))],
						ValueTypeMarker.Int16 => buffer[..(sizeof(int) + count * sizeof(short))],
						ValueTypeMarker.Int32 => buffer[..(sizeof(int) + count * sizeof(int))],
						ValueTypeMarker.Int64 => buffer[..(sizeof(int) + count * sizeof(long))],
						ValueTypeMarker.UInt8 => buffer[..(sizeof(int) + count * sizeof(byte))],
						ValueTypeMarker.UInt16 => buffer[..(sizeof(int) + count * sizeof(ushort))],
						ValueTypeMarker.UInt32 => buffer[..(sizeof(int) + count * sizeof(uint))],
						ValueTypeMarker.UInt64 => buffer[..(sizeof(int) + count * sizeof(ulong))],
						ValueTypeMarker.Float32 => buffer[..(sizeof(int) + count * sizeof(float))],
						ValueTypeMarker.Float64 => buffer[..(sizeof(int) + count * sizeof(double))],
						ValueTypeMarker.DateTime => buffer[..(sizeof(int) + count * sizeof(ulong))],
						ValueTypeMarker.Boolean => buffer[..(sizeof(int) + count * sizeof(byte))],
						_ => throw new NotImplementedException()
					};
			}
		}
	}
}
