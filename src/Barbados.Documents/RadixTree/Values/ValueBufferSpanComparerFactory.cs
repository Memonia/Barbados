using System;

using Barbados.Documents.RadixTree.Values.ValueBufferSpanComparers;

namespace Barbados.Documents.RadixTree.Values
{
	internal static class ValueBufferSpanComparerFactory
	{
		private static readonly FixedLengthTypeValueBufferSpanComparer<sbyte> _i8 = new(ValueBufferRawHelpers.ReadInt8);
		private static readonly FixedLengthTypeValueBufferSpanComparer<short> _i16 = new(ValueBufferRawHelpers.ReadInt16);
		private static readonly FixedLengthTypeValueBufferSpanComparer<int> _i32 = new(ValueBufferRawHelpers.ReadInt32);
		private static readonly FixedLengthTypeValueBufferSpanComparer<long> _i64 = new(ValueBufferRawHelpers.ReadInt64);
		private static readonly FixedLengthTypeValueBufferSpanComparer<byte> _ui8 = new(ValueBufferRawHelpers.ReadUInt8);
		private static readonly FixedLengthTypeValueBufferSpanComparer<ushort> _ui16 = new(ValueBufferRawHelpers.ReadUInt16);
		private static readonly FixedLengthTypeValueBufferSpanComparer<uint> _ui32 = new(ValueBufferRawHelpers.ReadUInt32);
		private static readonly FixedLengthTypeValueBufferSpanComparer<ulong> _ui64 = new(ValueBufferRawHelpers.ReadUInt64);
		private static readonly FixedLengthTypeValueBufferSpanComparer<float> _f32 = new(ValueBufferRawHelpers.ReadFloat32);
		private static readonly FixedLengthTypeValueBufferSpanComparer<double> _f64 = new(ValueBufferRawHelpers.ReadFloat64);
		private static readonly FixedLengthTypeValueBufferSpanComparer<DateTime> _dt = new(ValueBufferRawHelpers.ReadDateTime);
		private static readonly FixedLengthTypeValueBufferSpanComparer<bool> _b = new(ValueBufferRawHelpers.ReadBoolean);
		private static readonly ValueStringBufferSpanComparer _str = new();

		private static readonly FixedLengthTypeArrayBufferSpanComparer<sbyte> _ai8 = new(ValueTypeMarker.ArrayInt8, _i8);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<short> _ai16 = new(ValueTypeMarker.ArrayInt16, _i16);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<int> _ai32 = new(ValueTypeMarker.ArrayInt32, _i32);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<long> _ai64 = new(ValueTypeMarker.ArrayInt64, _i64);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<byte> _aui8 = new(ValueTypeMarker.ArrayUInt8, _ui8);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<ushort> _aui16 = new(ValueTypeMarker.ArrayUInt16, _ui16);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<uint> _aui32 = new(ValueTypeMarker.ArrayUInt32, _ui32);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<ulong> _aui64 = new(ValueTypeMarker.ArrayUInt64, _ui64);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<float> _af32 = new(ValueTypeMarker.ArrayFloat32, _f32);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<double> _af64 = new(ValueTypeMarker.ArrayFloat64, _f64);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<DateTime> _adt = new(ValueTypeMarker.ArrayDateTime, _dt);
		private static readonly FixedLengthTypeArrayBufferSpanComparer<bool> _ab = new(ValueTypeMarker.ArrayBoolean, _b);
		private static readonly ArrayStringBufferSpanComparer _astr = new();

		public static IValueBufferSpanComparer GetComparer(ValueTypeMarker marker)
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
				ValueTypeMarker.DateTime => _dt,
				ValueTypeMarker.Boolean => _b,
				ValueTypeMarker.String => _str,
				ValueTypeMarker.ArrayInt8 => _ai8,
				ValueTypeMarker.ArrayInt16 => _ai16,
				ValueTypeMarker.ArrayInt32 => _ai32,
				ValueTypeMarker.ArrayInt64 => _ai64,
				ValueTypeMarker.ArrayUInt8 => _aui8,
				ValueTypeMarker.ArrayUInt16 => _aui16,
				ValueTypeMarker.ArrayUInt32 => _aui32,
				ValueTypeMarker.ArrayUInt64 => _aui64,
				ValueTypeMarker.ArrayFloat32 => _af32,
				ValueTypeMarker.ArrayFloat64 => _af64,
				ValueTypeMarker.ArrayDateTime => _adt,
				ValueTypeMarker.ArrayBoolean => _ab,
				ValueTypeMarker.ArrayString => _astr,
				_ => throw new NotImplementedException()
			};
		}
	}
}
