using System;
using System.Buffers.Binary;
using System.Text;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed class NormalisedValue(byte[] bytes)
	{
		public static NormalisedValue Min => new([(byte)NormalisedValueType.Min]);
		public static NormalisedValue Max => new([(byte)NormalisedValueType.Max]);

		public static bool AreSameValueTypeOrInvalid(NormalisedValueSpan x, NormalisedValueSpan y)
		{
			var xType = (NormalisedValueType)x.Bytes[0];
			var yType = (NormalisedValueType)y.Bytes[0];
			if (
				xType == NormalisedValueType.Min || xType == NormalisedValueType.Max ||
				yType == NormalisedValueType.Min || yType == NormalisedValueType.Max
			)
			{
				return true;
			}

			return xType == yType;
		}

		public static NormalisedValue Create<T>(T value)
		{
			switch (value)
			{
				case byte ui8:
					var ui8n = new byte[sizeof(byte) + 1] { (byte)NormalisedValueType.UInt8, ui8 };
					return new NormalisedValue(ui8n);

				case sbyte i8:
					var i8n = new byte[sizeof(sbyte) + 1] { (byte)NormalisedValueType.Int8, (byte)(i8 ^ 0x80) };
					return new NormalisedValue(i8n);

				case bool ui1:
					var ui1n = new byte[sizeof(bool) + 1] { (byte)NormalisedValueType.Boolean, ui1 ? (byte)1 : (byte)0 };
					return new NormalisedValue(ui1n);

				case ushort ui16:
					var ui16n = new byte[sizeof(ushort) + 1];
					ui16n[0] = (byte)NormalisedValueType.UInt16;
					BinaryPrimitives.WriteUInt16BigEndian(ui16n.AsSpan()[1..], ui16);
					return new NormalisedValue(ui16n);

				case short i16:
					var i16n = new byte[sizeof(short) + 1];
					i16n[0] = (byte)NormalisedValueType.Int16;
					BinaryPrimitives.WriteUInt16BigEndian(i16n.AsSpan()[1..], (ushort)(i16 ^ 0x8000));
					return new NormalisedValue(i16n);

				case uint ui32:
					var ui32n = new byte[sizeof(uint) + 1];
					ui32n[0] = (byte)NormalisedValueType.UInt32;
					BinaryPrimitives.WriteUInt32BigEndian(ui32n.AsSpan()[1..], ui32);
					return new NormalisedValue(ui32n);

				case int i32:
					var i32n = new byte[sizeof(int) + 1];
					i32n[0] = (byte)NormalisedValueType.Int32;
					BinaryPrimitives.WriteUInt32BigEndian(i32n.AsSpan()[1..], (uint)i32 ^ 0x8000_0000);
					return new NormalisedValue(i32n);

				case ulong ui64:
					var ui64n = new byte[sizeof(ulong) + 1];
					ui64n[0] = (byte)NormalisedValueType.UInt64;
					BinaryPrimitives.WriteUInt64BigEndian(ui64n.AsSpan()[1..], ui64);
					return new NormalisedValue(ui64n);

				case long i64:
					var i64n = new byte[sizeof(long) + 1];
					i64n[0] = (byte)NormalisedValueType.Int64;
					BinaryPrimitives.WriteUInt64BigEndian(i64n.AsSpan()[1..], (ulong)i64 ^ 0x8000_0000_0000_0000);
					return new NormalisedValue(i64n);

				case float f32:
					var f32n = new byte[sizeof(float) + 1];
					f32n[0] = (byte)NormalisedValueType.Float32;
					BinaryPrimitives.WriteSingleBigEndian(
						f32n.AsSpan()[1..],
						f32 >= 0
							? BitConverter.SingleToUInt32Bits(f32) ^ 0x8000_0000
							: BitConverter.SingleToUInt32Bits(f32) ^ 0xFFFF_FFFF
					);
					return new NormalisedValue(f32n);

				case double f64:
					var f64n = new byte[sizeof(double) + 1];
					f64n[0] = (byte)NormalisedValueType.Float64;
					BinaryPrimitives.WriteDoubleBigEndian(
						f64n.AsSpan()[1..],
						f64 >= 0
							? BitConverter.DoubleToUInt64Bits(f64) ^ 0x8000_0000_0000_0000
							: BitConverter.DoubleToUInt64Bits(f64) ^ 0xFFFF_FFFF_FFFF_FFFF
					);
					return new NormalisedValue(f64n);

				case DateTime dt:
					var dtn = new byte[sizeof(long) + 1];
					dtn[0] = (byte)NormalisedValueType.DateTime;
					BinaryPrimitives.WriteUInt64BigEndian(dtn.AsSpan()[1..], (ulong)dt.Ticks ^ 0x8000_0000_0000_0000);
					return new NormalisedValue(dtn);

				case string str:
					var strn = new byte[Encoding.UTF8.GetByteCount(str) + 1];
					strn[0] = (byte)NormalisedValueType.String;
					Encoding.UTF8.GetBytes(str, strn.AsSpan()[1..]);
					return new NormalisedValue(strn);

				default:
					throw new ArgumentException($"Unsupported type {value?.GetType()}", nameof(value));
			}
		}

		private readonly byte[] _bytes = bytes;

		public NormalisedValueSpan AsSpan() => NormalisedValueSpan.FromNormalised(_bytes);
	}
}
