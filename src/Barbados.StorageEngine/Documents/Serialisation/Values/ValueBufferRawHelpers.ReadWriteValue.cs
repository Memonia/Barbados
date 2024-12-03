using System;
using System.Buffers.Binary;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal partial class ValueBufferRawHelpers
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
	}
}
