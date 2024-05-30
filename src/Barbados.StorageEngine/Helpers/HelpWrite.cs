using System;
using System.Buffers.Binary;

using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Helpers
{
	internal static class HelpWrite
	{
		public static void AsInt32(Span<byte> target, int value) => BinaryPrimitives.WriteInt32LittleEndian(target, value);
		public static void AsInt64(Span<byte> target, long value) => BinaryPrimitives.WriteInt64LittleEndian(target, value);
		public static void AsUInt32(Span<byte> target, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(target, value);
		public static void AsUInt64(Span<byte> target, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(target, value);

		public static void AsObjectId(Span<byte> target, ObjectId value) => AsInt64(target, value.Value);
		public static void AsPageHandle(Span<byte> target, PageHandle value) => AsInt64(target, value.Handle);
		public static void AsPageMarker(Span<byte> target, PageMarker value) => target[0] = (byte)value;
	}
}
