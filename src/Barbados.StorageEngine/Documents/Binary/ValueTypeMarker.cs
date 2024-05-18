using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal static class ValueTypeMarkerExtensions
	{
		public static int GetFixedLength(this ValueTypeMarker marker)
		{
			return marker switch
			{
				ValueTypeMarker.Int8 => sizeof(sbyte),
				ValueTypeMarker.Int16 => sizeof(short),
				ValueTypeMarker.Int32 => sizeof(int),
				ValueTypeMarker.Int64 => sizeof(long),
				ValueTypeMarker.UInt8 => sizeof(byte),
				ValueTypeMarker.UInt16 => sizeof(ushort),
				ValueTypeMarker.UInt32 => sizeof(uint),
				ValueTypeMarker.UInt64 => sizeof(ulong),
				ValueTypeMarker.Float32 => sizeof(float),
				ValueTypeMarker.Float64 => sizeof(double),
				ValueTypeMarker.Boolean => sizeof(bool),
				ValueTypeMarker.DateTime => sizeof(long),
				ValueTypeMarker.String => throw new InvalidOperationException(),
				_ => throw new NotImplementedException()
			};
		}

		public static bool IsVariableLength(this ValueTypeMarker marker)
		{
			return marker == ValueTypeMarker.String;
		}
	}

	internal enum ValueTypeMarker : byte
	{
		/* Normalised values of different types are sorted according to the marker's value */

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
		String
	}
}
