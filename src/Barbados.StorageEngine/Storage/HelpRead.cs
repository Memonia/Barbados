using System;
using System.Buffers.Binary;

using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Storage
{
	internal static class HelpRead
	{
		public static int AsInt32(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt32LittleEndian(source);
		public static long AsInt64(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt64LittleEndian(source);
		public static uint AsUInt32(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt32LittleEndian(source);
		public static ulong AsUInt64(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt64LittleEndian(source);

		public static ObjectId AsObjectId(ReadOnlySpan<byte> source) => new(AsInt64(source));
		public static PageHandle AsPageHandle(ReadOnlySpan<byte> source) => new(AsInt64(source));
		public static PageMarker AsPageMarker(ReadOnlySpan<byte> source) => (PageMarker)source[0];
	}
}
