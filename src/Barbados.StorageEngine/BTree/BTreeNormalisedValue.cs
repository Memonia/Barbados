using System;
using System.Buffers.Binary;
using System.Text;

namespace Barbados.StorageEngine.BTree
{
	internal sealed class BTreeNormalisedValue
	{
		public static implicit operator BTreeNormalisedValueSpan(BTreeNormalisedValue value) => value.AsSpan();

		public static BTreeNormalisedValue Min => new([(byte)BTreeLookupKeyTypeMarker.Min]);
		public static BTreeNormalisedValue Max => new([(byte)BTreeLookupKeyTypeMarker.Max]);

		public static int GetLength<T>(T value, bool isKeyExternal)
		{
			if (value is BTreeNormalisedValue nv)
			{
				if (nv._bytes[0] == (byte)BTreeLookupKeyTypeMarker.External)
				{
					if (isKeyExternal)
					{
						return nv._bytes.Length;
					}

					return nv._bytes.Length - 1;
				}

				else
				{
					if (!isKeyExternal)
					{
						return nv._bytes.Length;
					}

					return nv._bytes.Length + 1;
				}
			}

			var fixedExtraLength = 1;
			if (isKeyExternal)
			{
				fixedExtraLength += 1;
			}

			return fixedExtraLength + value switch
			{
				byte _ => sizeof(byte),
				sbyte _ => sizeof(sbyte),
				bool _ => sizeof(bool),
				ushort _ => sizeof(ushort),
				short _ => sizeof(short),
				uint _ => sizeof(uint),
				int _ => sizeof(int),
				ulong _ => sizeof(ulong),
				long _ => sizeof(long),
				float _ => sizeof(float),
				double _ => sizeof(double),
				DateTime _ => sizeof(long),
				string str => Encoding.UTF8.GetByteCount(str),
				_ => throw new ArgumentException($"Unsupported type {value?.GetType()}", nameof(value)),
			};
		}

		public static BTreeNormalisedValue Create<T>(T value, bool isKeyExternal)
		{
			var arr = new byte[GetLength(value, isKeyExternal)];
			var bytes = arr.AsSpan();
			if (isKeyExternal)
			{
				arr[0] = (byte)BTreeLookupKeyTypeMarker.External;
				bytes = arr.AsSpan()[1..];
			}

			switch (value)
			{
				case BTreeNormalisedValue nv:
					if (nv._bytes[0] == (byte)BTreeLookupKeyTypeMarker.External)
					{
						nv._bytes.AsSpan(1).CopyTo(bytes);
					}

					else
					{
						nv._bytes.CopyTo(bytes);
					}

					break;

				case byte ui8:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.UInt8;
					bytes[1] = ui8;
					break;

				case sbyte i8:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.Int8;
					bytes[1] = (byte)(i8 ^ 0x80);
					break;

				case bool ui1:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.Boolean;
					bytes[1] = ui1 ? (byte)1 : (byte)0;
					break;

				case ushort ui16:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.UInt16;
					BinaryPrimitives.WriteUInt16BigEndian(bytes[1..], ui16);
					break;

				case short i16:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.Int16;
					BinaryPrimitives.WriteUInt16BigEndian(bytes[1..], (ushort)(i16 ^ 0x8000));
					break;

				case uint ui32:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.UInt32;
					BinaryPrimitives.WriteUInt32BigEndian(bytes[1..], ui32);
					break;

				case int i32:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.Int32;
					WriteWithoutMarker(i32, bytes[1..]);
					break;

				case ulong ui64:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.UInt64;
					BinaryPrimitives.WriteUInt64BigEndian(bytes[1..], ui64);
					break;

				case long i64:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.Int64;
					WriteWithoutMarker(i64, bytes[1..]);
					break;

				case float f32:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.Float32;
					BinaryPrimitives.WriteSingleBigEndian(
						bytes[1..],
						f32 >= 0
							? BitConverter.SingleToUInt32Bits(f32) ^ 0x8000_0000
							: BitConverter.SingleToUInt32Bits(f32) ^ 0xFFFF_FFFF
					);

					break;

				case double f64:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.Float64;
					BinaryPrimitives.WriteDoubleBigEndian(
						bytes[1..],
						f64 >= 0
							? BitConverter.DoubleToUInt64Bits(f64) ^ 0x8000_0000_0000_0000
							: BitConverter.DoubleToUInt64Bits(f64) ^ 0xFFFF_FFFF_FFFF_FFFF
					);

					break;

				case DateTime dt:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.DateTime;
					BinaryPrimitives.WriteUInt64BigEndian(bytes[1..], (ulong)dt.Ticks ^ 0x8000_0000_0000_0000);
					break;

				case string str:
					bytes[0] = (byte)BTreeLookupKeyTypeMarker.String;
					Encoding.UTF8.GetBytes(str, bytes[1..]);
					break;

				default:
					throw new ArgumentException($"Unsupported type {value?.GetType()}", nameof(value));
			}

			return new(arr);
		}

		public static void WriteWithoutMarker(int value, Span<byte> destination)
		{
			BinaryPrimitives.WriteUInt32BigEndian(destination, (uint)value ^ 0x8000_0000);
		}

		public static void WriteWithoutMarker(long value, Span<byte> destination)
		{
			BinaryPrimitives.WriteUInt64BigEndian(destination, (ulong)value ^ 0x8000_0000_0000_0000);
		}

		private readonly byte[] _bytes;

		public BTreeNormalisedValue(byte[] bytes)
		{
			_bytes = bytes;
		}

		public BTreeNormalisedValueSpan AsSpan() => BTreeNormalisedValueSpan.FromNormalised(_bytes);
	}
}
