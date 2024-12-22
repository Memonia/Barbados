using System;

namespace Barbados.Documents.Serialisation.Values
{
	internal static class ValueTypeMarkerExtensions
	{
		public static int GetFixedLengthTypeLength(this ValueTypeMarker marker)
		{
			return marker switch
			{
				ValueTypeMarker.Int8 or ValueTypeMarker.ArrayInt8 => sizeof(sbyte),
				ValueTypeMarker.Int16 or ValueTypeMarker.ArrayInt16 => sizeof(short),
				ValueTypeMarker.Int32 or ValueTypeMarker.ArrayInt32 => sizeof(int),
				ValueTypeMarker.Int64 or ValueTypeMarker.ArrayInt64 => sizeof(long),
				ValueTypeMarker.UInt8 or ValueTypeMarker.ArrayUInt8 => sizeof(byte),
				ValueTypeMarker.UInt16 or ValueTypeMarker.ArrayUInt16 => sizeof(ushort),
				ValueTypeMarker.UInt32 or ValueTypeMarker.ArrayUInt32 => sizeof(uint),
				ValueTypeMarker.UInt64 or ValueTypeMarker.ArrayUInt64 => sizeof(ulong),
				ValueTypeMarker.Float32 or ValueTypeMarker.ArrayFloat32 => sizeof(float),
				ValueTypeMarker.Float64 or ValueTypeMarker.ArrayFloat64 => sizeof(double),
				ValueTypeMarker.DateTime or ValueTypeMarker.ArrayDateTime => sizeof(long),
				ValueTypeMarker.Boolean or ValueTypeMarker.ArrayBoolean => sizeof(bool),
				_ => throw new InvalidOperationException()
			};
		}

		public static bool IsArray(this ValueTypeMarker marker)
		{
			return marker >= ValueTypeMarker.ArrayInt8;
		}

		public static bool IsVariableLengthTypeValue(this ValueTypeMarker marker)
		{
			return marker == ValueTypeMarker.String;
		}

		public static bool IsVariableLengthTypeArray(this ValueTypeMarker marker)
		{
			return marker == ValueTypeMarker.ArrayString;
		}
	}

	internal enum ValueTypeMarker : byte
	{
		Int8 = 1,
		Int16,
		Int32,
		Int64,
		UInt8,
		UInt16,
		UInt32,
		UInt64,
		Float32,
		Float64,
		DateTime,
		Boolean,
		String,

		ArrayInt8 = 128,
		ArrayInt16,
		ArrayInt32,
		ArrayInt64,
		ArrayUInt8,
		ArrayUInt16,
		ArrayUInt32,
		ArrayUInt64,
		ArrayFloat32,
		ArrayFloat64,
		ArrayDateTime,
		ArrayBoolean,
		ArrayString
	}
}
