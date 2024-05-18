using System;

using Barbados.StorageEngine.Documents.Binary.ValueSpanComparers;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal static class ValueSpanComparerFactory
	{
		private static readonly ValueInt8SpanComparer _i8 = new();
		private static readonly ValueInt16SpanComparer _i16 = new();
		private static readonly ValueInt32SpanComparer _i32 = new();
		private static readonly ValueInt64SpanComparer _i64 = new();
		private static readonly ValueUInt8SpanComparer _ui8 = new();
		private static readonly ValueUInt16SpanComparer _ui16 = new();
		private static readonly ValueUInt32SpanComparer _ui32 = new();
		private static readonly ValueUInt64SpanComparer _ui64 = new();
		private static readonly ValueFloat32SpanComparer _f32 = new();
		private static readonly ValueFloat64SpanComparer _f64 = new();
		private static readonly ValueDateTimeSpanComparer _datetime = new();
		private static readonly ValueBooleanSpanComparer _boolean = new();
		private static readonly ValueStringSpanComparer _string = new();

		public static IValueSpanComparer GetComparer(ValueTypeMarker marker)
		{
			return marker switch
			{
				ValueTypeMarker.Int8 => _i8,
				ValueTypeMarker.Int16 => _i16,
				ValueTypeMarker.Int32 => _i32,
				ValueTypeMarker.Int64 => _i64,
				ValueTypeMarker.UInt8 => _ui8,
				ValueTypeMarker.UInt16 => _ui16,
				ValueTypeMarker.UInt32 => _ui32,
				ValueTypeMarker.UInt64 => _ui64,
				ValueTypeMarker.Float32 => _f32,
				ValueTypeMarker.Float64 => _f64,
				ValueTypeMarker.DateTime => _datetime,
				ValueTypeMarker.Boolean => _boolean,
				ValueTypeMarker.String => _string,
				_ => throw new ArgumentException($"Unsupported 'ValueTypeMarker' {marker}")
			};
		}
	}
}
