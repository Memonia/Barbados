using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Barbados.Documents.Json;
using Barbados.Documents.Serialisation;
using Barbados.Documents.Serialisation.Values;

namespace Barbados.Documents
{
	public sealed partial class BarbadosDocument
	{
		public static BarbadosDocument Empty { get; }

		static BarbadosDocument()
		{
			Empty = new Builder().Build();
		}

		private readonly RadixTreeBuffer _buffer;

		internal BarbadosDocument(RadixTreeBuffer buffer)
		{
			_buffer = buffer;
		}

		public int Count() => _buffer.Count();

		public KeyEnumerator GetKeyEnumerator() => new(this, flat: true);
		public KeyEnumerator GetFlatKeyEnumerator() => new(this, flat: false);
		public KeyStringEnumerator GetKeyStringEnumerator() => new(this, flat: true);
		public KeyStringEnumerator GetFlatKeyStringEnumerator() => new(this, flat: false);

		public bool HasField(BarbadosKey field)
		{
			return _buffer.ValueExists(field.SearchPrefix.AsBytes());
		}

		public bool HasDocument(BarbadosKey document)
		{
			return _buffer.PrefixExists(document.SearchPrefix.AsBytes());
		}

		public bool TryGetArrayCount(BarbadosKey field, out int count)
		{
			return _buffer.TryGetArrayBufferItemCount(field.SearchPrefix.AsBytes(), out count);
		}

		public bool TryGet(BarbadosKey field, out object value)
		{
			if (!_buffer.ValueExists(field.SearchPrefix.AsBytes(), out var marker))
			{
				if (TryGetDocument(field, out var doc))
				{
					value = doc;
					return true;
				}

				if (TryGetDocumentArray(field, out var docs))
				{
					value = docs;
					return true;
				}

				value = default!;
				return false;
			}

			switch (marker)
			{
				case ValueTypeMarker.Int8 when _buffer.TryGetInt8(field.SearchPrefix.AsBytes(), out var i8):
					value = i8;
					return true;

				case ValueTypeMarker.Int16 when _buffer.TryGetInt16(field.SearchPrefix.AsBytes(), out var i16):
					value = i16;
					return true;

				case ValueTypeMarker.Int32 when _buffer.TryGetInt32(field.SearchPrefix.AsBytes(), out var i32):
					value = i32;
					return true;

				case ValueTypeMarker.Int64 when _buffer.TryGetInt64(field.SearchPrefix.AsBytes(), out var i64):
					value = i64;
					return true;

				case ValueTypeMarker.UInt8 when _buffer.TryGetUInt8(field.SearchPrefix.AsBytes(), out var ui8):
					value = ui8;
					return true;

				case ValueTypeMarker.UInt16 when _buffer.TryGetUInt16(field.SearchPrefix.AsBytes(), out var ui16):
					value = ui16;
					return true;

				case ValueTypeMarker.UInt32 when _buffer.TryGetUInt32(field.SearchPrefix.AsBytes(), out var ui32):
					value = ui32;
					return true;

				case ValueTypeMarker.UInt64 when _buffer.TryGetUInt64(field.SearchPrefix.AsBytes(), out var ui64):
					value = ui64;
					return true;

				case ValueTypeMarker.Float32 when _buffer.TryGetFloat32(field.SearchPrefix.AsBytes(), out var f32):
					value = f32;
					return true;

				case ValueTypeMarker.Float64 when _buffer.TryGetFloat64(field.SearchPrefix.AsBytes(), out var f64):
					value = f64;
					return true;

				case ValueTypeMarker.DateTime when _buffer.TryGetDateTime(field.SearchPrefix.AsBytes(), out var dt):
					value = dt;
					return true;

				case ValueTypeMarker.Boolean when _buffer.TryGetBoolean(field.SearchPrefix.AsBytes(), out var b):
					value = b;
					return true;

				case ValueTypeMarker.String when _buffer.TryGetString(field.SearchPrefix.AsBytes(), out var s):
					value = s;
					return true;

				case ValueTypeMarker.ArrayInt8 when _buffer.TryGetInt8Array(field.SearchPrefix.AsBytes(), out var i8a):
					value = i8a;
					return true;

				case ValueTypeMarker.ArrayInt16 when _buffer.TryGetInt16Array(field.SearchPrefix.AsBytes(), out var i16a):
					value = i16a;
					return true;

				case ValueTypeMarker.ArrayInt32 when _buffer.TryGetInt32Array(field.SearchPrefix.AsBytes(), out var i32a):
					value = i32a;
					return true;

				case ValueTypeMarker.ArrayInt64 when _buffer.TryGetInt64Array(field.SearchPrefix.AsBytes(), out var i64a):
					value = i64a;
					return true;

				case ValueTypeMarker.ArrayUInt8 when _buffer.TryGetUInt8Array(field.SearchPrefix.AsBytes(), out var ui8a):
					value = ui8a;
					return true;

				case ValueTypeMarker.ArrayUInt16 when _buffer.TryGetUInt16Array(field.SearchPrefix.AsBytes(), out var ui16a):
					value = ui16a;
					return true;

				case ValueTypeMarker.ArrayUInt32 when _buffer.TryGetUInt32Array(field.SearchPrefix.AsBytes(), out var ui32a):
					value = ui32a;
					return true;

				case ValueTypeMarker.ArrayUInt64 when _buffer.TryGetUInt64Array(field.SearchPrefix.AsBytes(), out var ui64a):
					value = ui64a;
					return true;

				case ValueTypeMarker.ArrayFloat32 when _buffer.TryGetFloat32Array(field.SearchPrefix.AsBytes(), out var f32a):
					value = f32a;
					return true;

				case ValueTypeMarker.ArrayFloat64 when _buffer.TryGetFloat64Array(field.SearchPrefix.AsBytes(), out var f64a):
					value = f64a;
					return true;

				case ValueTypeMarker.ArrayDateTime when _buffer.TryGetDateTimeArray(field.SearchPrefix.AsBytes(), out var dta):
					value = dta;
					return true;

				case ValueTypeMarker.ArrayBoolean when _buffer.TryGetBooleanArray(field.SearchPrefix.AsBytes(), out var ba):
					value = ba;
					return true;

				case ValueTypeMarker.ArrayString when _buffer.TryGetStringArray(field.SearchPrefix.AsBytes(), out var sa):
					value = sa;
					return true;

				default:
					Debug.Assert(false);
					throw new NotImplementedException();
			}
		}

		public bool TryGetWrapped(BarbadosKey field, out BarbadosDocument extracted)
		{
			if (field.IsDocument)
			{
				return TryGetDocument(field, out extracted);
			}

			if (_buffer.TryGetBuffer(field.SearchPrefix.AsBytes(), out var valueBuffer))
			{
				var buffer = new RadixTreeBuffer.Builder()
					.AddBuffer(field.SearchPrefix, valueBuffer)
					.Build();

				extracted = new BarbadosDocument(buffer);
				return true;
			}

			extracted = default!;
			return false;
		}

		public bool TryGetInt8(BarbadosKey field, out sbyte value) => _buffer.TryGetInt8(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetInt16(BarbadosKey field, out short value) => _buffer.TryGetInt16(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetInt32(BarbadosKey field, out int value) => _buffer.TryGetInt32(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetInt64(BarbadosKey field, out long value) => _buffer.TryGetInt64(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetUInt8(BarbadosKey field, out byte value) => _buffer.TryGetUInt8(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetUInt16(BarbadosKey field, out ushort value) => _buffer.TryGetUInt16(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetUInt32(BarbadosKey field, out uint value) => _buffer.TryGetUInt32(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetUInt64(BarbadosKey field, out ulong value) => _buffer.TryGetUInt64(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetFloat32(BarbadosKey field, out float value) => _buffer.TryGetFloat32(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetFloat64(BarbadosKey field, out double value) => _buffer.TryGetFloat64(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetDateTime(BarbadosKey field, out DateTime value) => _buffer.TryGetDateTime(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetBoolean(BarbadosKey field, out bool value) => _buffer.TryGetBoolean(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetString(BarbadosKey field, out string value) => _buffer.TryGetString(field.SearchPrefix.AsBytes(), out value);
		public bool TryGetInt8Array(BarbadosKey field, out sbyte[] array) => _buffer.TryGetInt8Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetInt16Array(BarbadosKey field, out short[] array) => _buffer.TryGetInt16Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetInt32Array(BarbadosKey field, out int[] array) => _buffer.TryGetInt32Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetInt64Array(BarbadosKey field, out long[] array) => _buffer.TryGetInt64Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetUInt8Array(BarbadosKey field, out byte[] array) => _buffer.TryGetUInt8Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetUInt16Array(BarbadosKey field, out ushort[] array) => _buffer.TryGetUInt16Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetUInt32Array(BarbadosKey field, out uint[] array) => _buffer.TryGetUInt32Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetUInt64Array(BarbadosKey field, out ulong[] array) => _buffer.TryGetUInt64Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetFloat32Array(BarbadosKey field, out float[] array) => _buffer.TryGetFloat32Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetFloat64Array(BarbadosKey field, out double[] array) => _buffer.TryGetFloat64Array(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetDateTimeArray(BarbadosKey field, out DateTime[] array) => _buffer.TryGetDateTimeArray(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetBooleanArray(BarbadosKey field, out bool[] array) => _buffer.TryGetBooleanArray(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetStringArray(BarbadosKey field, out string[] array) => _buffer.TryGetStringArray(field.SearchPrefix.AsBytes(), out array);
		public bool TryGetInt8FromArray(BarbadosKey field, int index, out sbyte value) => _buffer.TryGetFromInt8Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetInt16FromArray(BarbadosKey field, int index, out short value) => _buffer.TryGetFromInt16Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetInt32FromArray(BarbadosKey field, int index, out int value) => _buffer.TryGetFromInt32Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetInt64FromArray(BarbadosKey field, int index, out long value) => _buffer.TryGetFromInt64Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetUInt8FromArray(BarbadosKey field, int index, out byte value) => _buffer.TryGetFromUInt8Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetUInt16FromArray(BarbadosKey field, int index, out ushort value) => _buffer.TryGetFromUInt16Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetUInt32FromArray(BarbadosKey field, int index, out uint value) => _buffer.TryGetFromUInt32Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetUInt64FromArray(BarbadosKey field, int index, out ulong value) => _buffer.TryGetFromUInt64Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetFloat32FromArray(BarbadosKey field, int index, out float value) => _buffer.TryGetFromFloat32Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetFloat64FromArray(BarbadosKey field, int index, out double value) => _buffer.TryGetFromFloat64Array(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetDateTimeFromArray(BarbadosKey field, int index, out DateTime value) => _buffer.TryGetFromDateTimeArray(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetBooleanFromArray(BarbadosKey field, int index, out bool value) => _buffer.TryGetFromBooleanArray(field.SearchPrefix.AsBytes(), index, out value);
		public bool TryGetStringFromArray(BarbadosKey field, int index, out string value) => _buffer.TryGetFromStringArray(field.SearchPrefix.AsBytes(), index, out value);

		public bool TryGetDocument(BarbadosKey field, out BarbadosDocument document)
		{
			if (!field.IsDocument)
			{
				field = field.GetDocumentKey();
			}

			if (_buffer.TryExtract(field.SearchPrefix.AsBytes(), out var buffer))
			{
				document = new BarbadosDocument(buffer);
				return true;
			}

			document = default!;
			return false;
		}

		public bool TryGetDocumentArray(BarbadosKey field, out BarbadosDocument[] documents)
		{
			var sb = new StringBuilder(field.ToString());
			var startLength = sb.Length;
			if (!field.IsDocument)
			{
				sb.Append(BarbadosKey.NestingSeparator);
			}
			sb.Append(0);
			sb.Append(BarbadosKey.NestingSeparator);

			BarbadosKey firstItemName = sb.ToString();
			if (!_buffer.PrefixExists(firstItemName.SearchPrefix.AsBytes()))
			{
				documents = default!;
				return false;
			}

			var l = new List<BarbadosDocument>();
			int count = 0;
			while (TryGetDocument(sb.ToString(), out var document))
			{
				l.Add(document);
				count += 1;
				sb.Length = startLength;
				sb.Append(count);
				sb.Append(BarbadosKey.NestingSeparator);
			}

			documents = [.. l];
			return true;
		}

		public bool TryGetDocumentFromArray(BarbadosKey field, int index, out BarbadosDocument document)
		{
			var sb = new StringBuilder($"{field}{BarbadosKey.NestingSeparator}{index}{BarbadosKey.NestingSeparator}");
			BarbadosKey itemName = sb.ToString();
			return TryGetDocument(itemName, out document);
		}

		public override string ToString()
		{
			return this.ToJson();
		}

		public ReadOnlySpan<byte> AsBytes() => _buffer.AsSpan();
	}
}
