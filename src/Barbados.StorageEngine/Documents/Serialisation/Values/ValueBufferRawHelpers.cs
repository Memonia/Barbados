using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal static partial class ValueBufferRawHelpers
	{
		public static int GetArrayBufferCount(ReadOnlySpan<byte> buffer)
		{
			return ReadInt32(buffer);
		}

		public static ReadOnlySpan<byte> GetBufferBytes(ReadOnlySpan<byte> source, ValueTypeMarker marker)
		{
			switch (marker)
			{
				case ValueTypeMarker.Int8:
				case ValueTypeMarker.UInt8:
				case ValueTypeMarker.Boolean:
					return source[..sizeof(byte)];

				case ValueTypeMarker.Int16:
				case ValueTypeMarker.UInt16:
					return source[..sizeof(ushort)];

				case ValueTypeMarker.UInt32:
				case ValueTypeMarker.Int32:
				case ValueTypeMarker.Float32:
					return source[..sizeof(uint)];

				case ValueTypeMarker.UInt64:
				case ValueTypeMarker.Int64:
				case ValueTypeMarker.Float64:
				case ValueTypeMarker.DateTime:
					return source[..sizeof(ulong)];

				case ValueTypeMarker.String:
					return source[..(sizeof(int) + ReadInt32(source))];

				case ValueTypeMarker.ArrayInt8:
				case ValueTypeMarker.ArrayUInt8:
				case ValueTypeMarker.ArrayBoolean:
					return source[..(sizeof(int) + GetArrayBufferCount(source) * sizeof(byte))];

				case ValueTypeMarker.ArrayInt16:
				case ValueTypeMarker.ArrayUInt16:
					return source[..(sizeof(int) + GetArrayBufferCount(source) * sizeof(ushort))];

				case ValueTypeMarker.ArrayInt32:
				case ValueTypeMarker.ArrayUInt32:
				case ValueTypeMarker.ArrayFloat32:
					return source[..(sizeof(int) + GetArrayBufferCount(source) * sizeof(uint))];

				case ValueTypeMarker.ArrayInt64:
				case ValueTypeMarker.ArrayUInt64:
				case ValueTypeMarker.ArrayFloat64:
				case ValueTypeMarker.ArrayDateTime:
					return source[..(sizeof(int) + GetArrayBufferCount(source) * sizeof(ulong))];

				case ValueTypeMarker.ArrayString:
					// See 'ValueVariableLengthBufferArray'
					var count = GetArrayBufferCount(source);
					var buffersLength = ReadInt32(source[(sizeof(int) + sizeof(int) * (count - 1))..]);
					return source[..(sizeof(int) + count * sizeof(int) + buffersLength)];

				default:
					throw new NotImplementedException();
			}
		}
	}
}
