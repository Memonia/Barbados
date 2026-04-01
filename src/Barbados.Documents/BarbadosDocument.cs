using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

using Barbados.Documents.Json;
using Barbados.Documents.RadixTree;
using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents
{
	public sealed partial class BarbadosDocument : IEquatable<BarbadosDocument>
	{
		public static BarbadosDocument Empty { get; } = new Builder().Build();

		private readonly RadixTreeBuffer _buffer;

		private BarbadosDocument(RadixTreeBuffer buffer)
		{
			_buffer = buffer;
		}

		public int Count() => _buffer.Count();

		public KeyEnumerator GetKeyEnumerator(bool flat) => new(this, flat);

		public bool TryGetDocumentKeyEnumerator(BarbadosKey key, bool flat, out KeyEnumerator enumerator)
		{
			if (!HasDocument(key))
			{
				enumerator = default;
				return false;
			}

			enumerator = new(this, flat, key);
			return true;
		}

		public bool Exists(BarbadosKey key)
		{
			return HasValue(key) || HasDocument(key);
		}

		public bool HasValue(BarbadosKey key)
		{
			return _buffer.ValueExists(key.ValueSearchPrefix);
		}

		public bool HasDocument(BarbadosKey key)
		{
			return _buffer.PrefixExists(key.DocumentSearchPrefix);
		}

		public bool TryGetArrayLength(BarbadosKey key, out int length)
		{
			return _buffer.TryGetArrayBufferItemCount(key.ValueSearchPrefix, out length);
		}

		public bool TryGet(BarbadosKey key, out object value)
		{
			if (!_buffer.ValueExists(key.ValueSearchPrefix, out var marker))
			{
				if (TryGetDocument(key, out var doc))
				{
					value = doc;
					return true;
				}

				else
				if (TryGetDocumentArray(key, out var docs))
				{
					value = docs;
					return true;
				}

				value = default!;
				return false;
			}

			switch (marker)
			{
				case ValueTypeMarker.Int8 when _buffer.TryGetInt8(key.ValueSearchPrefix, out var i8):
					value = i8;
					return true;

				case ValueTypeMarker.Int16 when _buffer.TryGetInt16(key.ValueSearchPrefix, out var i16):
					value = i16;
					return true;

				case ValueTypeMarker.Int32 when _buffer.TryGetInt32(key.ValueSearchPrefix, out var i32):
					value = i32;
					return true;

				case ValueTypeMarker.Int64 when _buffer.TryGetInt64(key.ValueSearchPrefix, out var i64):
					value = i64;
					return true;

				case ValueTypeMarker.UInt8 when _buffer.TryGetUInt8(key.ValueSearchPrefix, out var ui8):
					value = ui8;
					return true;

				case ValueTypeMarker.UInt16 when _buffer.TryGetUInt16(key.ValueSearchPrefix, out var ui16):
					value = ui16;
					return true;

				case ValueTypeMarker.UInt32 when _buffer.TryGetUInt32(key.ValueSearchPrefix, out var ui32):
					value = ui32;
					return true;

				case ValueTypeMarker.UInt64 when _buffer.TryGetUInt64(key.ValueSearchPrefix, out var ui64):
					value = ui64;
					return true;

				case ValueTypeMarker.Float32 when _buffer.TryGetFloat32(key.ValueSearchPrefix, out var f32):
					value = f32;
					return true;

				case ValueTypeMarker.Float64 when _buffer.TryGetFloat64(key.ValueSearchPrefix, out var f64):
					value = f64;
					return true;

				case ValueTypeMarker.DateTime when _buffer.TryGetDateTime(key.ValueSearchPrefix, out var dt):
					value = dt;
					return true;

				case ValueTypeMarker.Boolean when _buffer.TryGetBoolean(key.ValueSearchPrefix, out var b):
					value = b;
					return true;

				case ValueTypeMarker.String when _buffer.TryGetString(key.ValueSearchPrefix, out var s):
					value = s;
					return true;

				case ValueTypeMarker.ArrayInt8 when _buffer.TryGetInt8Array(key.ValueSearchPrefix, out var i8a):
					value = i8a;
					return true;

				case ValueTypeMarker.ArrayInt16 when _buffer.TryGetInt16Array(key.ValueSearchPrefix, out var i16a):
					value = i16a;
					return true;

				case ValueTypeMarker.ArrayInt32 when _buffer.TryGetInt32Array(key.ValueSearchPrefix, out var i32a):
					value = i32a;
					return true;

				case ValueTypeMarker.ArrayInt64 when _buffer.TryGetInt64Array(key.ValueSearchPrefix, out var i64a):
					value = i64a;
					return true;

				case ValueTypeMarker.ArrayUInt8 when _buffer.TryGetUInt8Array(key.ValueSearchPrefix, out var ui8a):
					value = ui8a;
					return true;

				case ValueTypeMarker.ArrayUInt16 when _buffer.TryGetUInt16Array(key.ValueSearchPrefix, out var ui16a):
					value = ui16a;
					return true;

				case ValueTypeMarker.ArrayUInt32 when _buffer.TryGetUInt32Array(key.ValueSearchPrefix, out var ui32a):
					value = ui32a;
					return true;

				case ValueTypeMarker.ArrayUInt64 when _buffer.TryGetUInt64Array(key.ValueSearchPrefix, out var ui64a):
					value = ui64a;
					return true;

				case ValueTypeMarker.ArrayFloat32 when _buffer.TryGetFloat32Array(key.ValueSearchPrefix, out var f32a):
					value = f32a;
					return true;

				case ValueTypeMarker.ArrayFloat64 when _buffer.TryGetFloat64Array(key.ValueSearchPrefix, out var f64a):
					value = f64a;
					return true;

				case ValueTypeMarker.ArrayDateTime when _buffer.TryGetDateTimeArray(key.ValueSearchPrefix, out var dta):
					value = dta;
					return true;

				case ValueTypeMarker.ArrayBoolean when _buffer.TryGetBooleanArray(key.ValueSearchPrefix, out var ba):
					value = ba;
					return true;

				case ValueTypeMarker.ArrayString when _buffer.TryGetStringArray(key.ValueSearchPrefix, out var sa):
					value = sa;
					return true;

				default:
					Debug.Assert(false);
					throw new NotImplementedException();
			}
		}

		public bool TryGetWrapped(BarbadosKey key, out BarbadosDocument extracted)
		{
			if (TryGetDocument(key, out extracted))
			{
				return true;
			}

			if (_buffer.TryGetBuffer(key.ValueSearchPrefix, out var valueBuffer))
			{
				var buffer = new RadixTreeBuffer.Builder()
					.AddBuffer(key.ValueSearchPrefix, valueBuffer)
					.Build();

				extracted = new BarbadosDocument(buffer);
				return true;
			}

			extracted = default!;
			return false;
		}

		public bool TryGetInt8(BarbadosKey key, out sbyte value) => _buffer.TryGetInt8(key.ValueSearchPrefix, out value);
		public bool TryGetInt16(BarbadosKey key, out short value) => _buffer.TryGetInt16(key.ValueSearchPrefix, out value);
		public bool TryGetInt32(BarbadosKey key, out int value) => _buffer.TryGetInt32(key.ValueSearchPrefix, out value);
		public bool TryGetInt64(BarbadosKey key, out long value) => _buffer.TryGetInt64(key.ValueSearchPrefix, out value);
		public bool TryGetUInt8(BarbadosKey key, out byte value) => _buffer.TryGetUInt8(key.ValueSearchPrefix, out value);
		public bool TryGetUInt16(BarbadosKey key, out ushort value) => _buffer.TryGetUInt16(key.ValueSearchPrefix, out value);
		public bool TryGetUInt32(BarbadosKey key, out uint value) => _buffer.TryGetUInt32(key.ValueSearchPrefix, out value);
		public bool TryGetUInt64(BarbadosKey key, out ulong value) => _buffer.TryGetUInt64(key.ValueSearchPrefix, out value);
		public bool TryGetFloat32(BarbadosKey key, out float value) => _buffer.TryGetFloat32(key.ValueSearchPrefix, out value);
		public bool TryGetFloat64(BarbadosKey key, out double value) => _buffer.TryGetFloat64(key.ValueSearchPrefix, out value);
		public bool TryGetDateTime(BarbadosKey key, out DateTime value) => _buffer.TryGetDateTime(key.ValueSearchPrefix, out value);
		public bool TryGetBoolean(BarbadosKey key, out bool value) => _buffer.TryGetBoolean(key.ValueSearchPrefix, out value);
		public bool TryGetString(BarbadosKey key, out string value) => _buffer.TryGetString(key.ValueSearchPrefix, out value);
		public bool TryGetInt8Array(BarbadosKey key, out sbyte[] array) => _buffer.TryGetInt8Array(key.ValueSearchPrefix, out array);
		public bool TryGetInt16Array(BarbadosKey key, out short[] array) => _buffer.TryGetInt16Array(key.ValueSearchPrefix, out array);
		public bool TryGetInt32Array(BarbadosKey key, out int[] array) => _buffer.TryGetInt32Array(key.ValueSearchPrefix, out array);
		public bool TryGetInt64Array(BarbadosKey key, out long[] array) => _buffer.TryGetInt64Array(key.ValueSearchPrefix, out array);
		public bool TryGetUInt8Array(BarbadosKey key, out byte[] array) => _buffer.TryGetUInt8Array(key.ValueSearchPrefix, out array);
		public bool TryGetUInt16Array(BarbadosKey key, out ushort[] array) => _buffer.TryGetUInt16Array(key.ValueSearchPrefix, out array);
		public bool TryGetUInt32Array(BarbadosKey key, out uint[] array) => _buffer.TryGetUInt32Array(key.ValueSearchPrefix, out array);
		public bool TryGetUInt64Array(BarbadosKey key, out ulong[] array) => _buffer.TryGetUInt64Array(key.ValueSearchPrefix, out array);
		public bool TryGetFloat32Array(BarbadosKey key, out float[] array) => _buffer.TryGetFloat32Array(key.ValueSearchPrefix, out array);
		public bool TryGetFloat64Array(BarbadosKey key, out double[] array) => _buffer.TryGetFloat64Array(key.ValueSearchPrefix, out array);
		public bool TryGetDateTimeArray(BarbadosKey key, out DateTime[] array) => _buffer.TryGetDateTimeArray(key.ValueSearchPrefix, out array);
		public bool TryGetBooleanArray(BarbadosKey key, out bool[] array) => _buffer.TryGetBooleanArray(key.ValueSearchPrefix, out array);
		public bool TryGetStringArray(BarbadosKey key, out string[] array) => _buffer.TryGetStringArray(key.ValueSearchPrefix, out array);
		public bool TryGetInt8FromArray(BarbadosKey key, int index, out sbyte value) => _buffer.TryGetFromInt8Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetInt16FromArray(BarbadosKey key, int index, out short value) => _buffer.TryGetFromInt16Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetInt32FromArray(BarbadosKey key, int index, out int value) => _buffer.TryGetFromInt32Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetInt64FromArray(BarbadosKey key, int index, out long value) => _buffer.TryGetFromInt64Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetUInt8FromArray(BarbadosKey key, int index, out byte value) => _buffer.TryGetFromUInt8Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetUInt16FromArray(BarbadosKey key, int index, out ushort value) => _buffer.TryGetFromUInt16Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetUInt32FromArray(BarbadosKey key, int index, out uint value) => _buffer.TryGetFromUInt32Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetUInt64FromArray(BarbadosKey key, int index, out ulong value) => _buffer.TryGetFromUInt64Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetFloat32FromArray(BarbadosKey key, int index, out float value) => _buffer.TryGetFromFloat32Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetFloat64FromArray(BarbadosKey key, int index, out double value) => _buffer.TryGetFromFloat64Array(key.ValueSearchPrefix, index, out value);
		public bool TryGetDateTimeFromArray(BarbadosKey key, int index, out DateTime value) => _buffer.TryGetFromDateTimeArray(key.ValueSearchPrefix, index, out value);
		public bool TryGetBooleanFromArray(BarbadosKey key, int index, out bool value) => _buffer.TryGetFromBooleanArray(key.ValueSearchPrefix, index, out value);
		public bool TryGetStringFromArray(BarbadosKey key, int index, out string value) => _buffer.TryGetFromStringArray(key.ValueSearchPrefix, index, out value);

		public bool TryGetDocument(BarbadosKey key, out BarbadosDocument document)
		{
			if (_buffer.TryExtract(key.DocumentSearchPrefix, out var buffer))
			{
				document = new BarbadosDocument(buffer);
				return true;
			}

			document = default!;
			return false;
		}

		public bool TryGetDocumentArray(BarbadosKey key, out BarbadosDocument[] documents)
		{
			var sb = new StringBuilder(key.DocumentSearchPrefix.ToString());
			var startLength = sb.Length;

			sb.Append(0);

			// Empty arrays are not stored. If zero-index item does not exist, then there is no array
			BarbadosKey currentItemKey = sb.ToString();
			if (!_buffer.PrefixExists(currentItemKey.DocumentSearchPrefix))
			{
				documents = default!;
				return false;
			}

			var l = new List<BarbadosDocument>();
			int count = 0;
			while (TryGetDocument(currentItemKey, out var document))
			{
				l.Add(document);
				count += 1;
				sb.Length = startLength;
				sb.Append(count);
				currentItemKey = sb.ToString();
			}

			documents = [.. l];
			return true;
		}

		public bool TryGetDocumentFromArray(BarbadosKey key, int index, out BarbadosDocument document)
		{
			var sb = new StringBuilder($"{key}{BarbadosKey.NestingSeparator}{index}{BarbadosKey.NestingSeparator}");
			var itemName = sb.ToString();
			return TryGetDocument(itemName, out document);
		}

		public override string ToString()
		{
			return this.ToJson();
		}

		public ReadOnlySpan<byte> AsBytes() => _buffer.AsSpan();
	}
}
