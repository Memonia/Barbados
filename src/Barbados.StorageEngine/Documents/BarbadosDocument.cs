using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Barbados.StorageEngine.Documents.Serialisation;
using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Documents
{
	public sealed partial class BarbadosDocument
	{
		public ObjectId Id { get; }

		internal RadixTreeBuffer Buffer { get; }

		internal BarbadosDocument(ObjectId id, RadixTreeBuffer buffer)
		{
			Id = id;
			Buffer = buffer;
		}

		public int Count() => Buffer.Count();

		public List<BarbadosIdentifier> GetFields()
		{
			var docs = new List<string>();
			var fields = new List<BarbadosIdentifier>();
			var e = Buffer.GetKeyValueEnumerator();
			while (e.TryGetNext(out var key))
			{
				BarbadosIdentifier k = key.ToString();

				// Is current field a part of any document identifier?
				var splitIndices = k.GetSplitIndices();
				BarbadosIdentifier doc;
				if (splitIndices.MoveNext() && splitIndices.Current < k.Identifier.Length)
				{
					doc = k.Identifier[..(splitIndices.Current + 1)];
					if (!docs.Contains((string)doc, StringComparer.Ordinal))
					{
						docs.Add(doc);
						fields.Add(doc.GetDocumentName());
					}
				}

				else
				{
					fields.Add(k);
				}
			}

			return fields;
		}

		public List<BarbadosIdentifier> GetFieldsDeep()
		{
			var fields = new List<BarbadosIdentifier>();
			var e = Buffer.GetKeyValueEnumerator();
			while (e.TryGetNext(out var key))
			{
				fields.Add(key.ToString());
			}

			return fields;
		}

		public bool HasField(BarbadosIdentifier field)
		{
			return Buffer.ValueExists(field.BinaryName.AsBytes());
		}

		public bool HasDocument(BarbadosIdentifier document)
		{
			return Buffer.PrefixExists(document.BinaryName.AsBytes());
		}

		public bool TryGetArrayCount(BarbadosIdentifier field, out int count)
		{
			return Buffer.TryGetArrayBufferItemCount(field.BinaryName.AsBytes(), out count);
		}

		public bool TryGet(BarbadosIdentifier field, out object value)
		{
			if (!Buffer.ValueExists(field.BinaryName.AsBytes(), out var marker))
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
				case ValueTypeMarker.Int8 when Buffer.TryGetInt8(field.BinaryName.AsBytes(), out var i8):
					value = i8;
					return true;

				case ValueTypeMarker.Int16 when Buffer.TryGetInt16(field.BinaryName.AsBytes(), out var i16):
					value = i16;
					return true;

				case ValueTypeMarker.Int32 when Buffer.TryGetInt32(field.BinaryName.AsBytes(), out var i32):
					value = i32;
					return true;

				case ValueTypeMarker.Int64 when Buffer.TryGetInt64(field.BinaryName.AsBytes(), out var i64):
					value = i64;
					return true;

				case ValueTypeMarker.UInt8 when Buffer.TryGetUInt8(field.BinaryName.AsBytes(), out var ui8):
					value = ui8;
					return true;

				case ValueTypeMarker.UInt16 when Buffer.TryGetUInt16(field.BinaryName.AsBytes(), out var ui16):
					value = ui16;
					return true;

				case ValueTypeMarker.UInt32 when Buffer.TryGetUInt32(field.BinaryName.AsBytes(), out var ui32):
					value = ui32;
					return true;

				case ValueTypeMarker.UInt64 when Buffer.TryGetUInt64(field.BinaryName.AsBytes(), out var ui64):
					value = ui64;
					return true;

				case ValueTypeMarker.Float32 when Buffer.TryGetFloat32(field.BinaryName.AsBytes(), out var f32):
					value = f32;
					return true;

				case ValueTypeMarker.Float64 when Buffer.TryGetFloat64(field.BinaryName.AsBytes(), out var f64):
					value = f64;
					return true;

				case ValueTypeMarker.DateTime when Buffer.TryGetDateTime(field.BinaryName.AsBytes(), out var dt):
					value = dt;
					return true;

				case ValueTypeMarker.Boolean when Buffer.TryGetBoolean(field.BinaryName.AsBytes(), out var b):
					value = b;
					return true;

				case ValueTypeMarker.String when Buffer.TryGetString(field.BinaryName.AsBytes(), out var s):
					value = s;
					return true;

				case ValueTypeMarker.ArrayInt8 when Buffer.TryGetInt8Array(field.BinaryName.AsBytes(), out var i8a):
					value = i8a;
					return true;

				case ValueTypeMarker.ArrayInt16 when Buffer.TryGetInt16Array(field.BinaryName.AsBytes(), out var i16a):
					value = i16a;
					return true;

				case ValueTypeMarker.ArrayInt32 when Buffer.TryGetInt32Array(field.BinaryName.AsBytes(), out var i32a):
					value = i32a;
					return true;

				case ValueTypeMarker.ArrayInt64 when Buffer.TryGetInt64Array(field.BinaryName.AsBytes(), out var i64a):
					value = i64a;
					return true;

				case ValueTypeMarker.ArrayUInt8 when Buffer.TryGetUInt8Array(field.BinaryName.AsBytes(), out var ui8a):
					value = ui8a;
					return true;

				case ValueTypeMarker.ArrayUInt16 when Buffer.TryGetUInt16Array(field.BinaryName.AsBytes(), out var ui16a):
					value = ui16a;
					return true;

				case ValueTypeMarker.ArrayUInt32 when Buffer.TryGetUInt32Array(field.BinaryName.AsBytes(), out var ui32a):
					value = ui32a;
					return true;

				case ValueTypeMarker.ArrayUInt64 when Buffer.TryGetUInt64Array(field.BinaryName.AsBytes(), out var ui64a):
					value = ui64a;
					return true;

				case ValueTypeMarker.ArrayFloat32 when Buffer.TryGetFloat32Array(field.BinaryName.AsBytes(), out var f32a):
					value = f32a;
					return true;

				case ValueTypeMarker.ArrayFloat64 when Buffer.TryGetFloat64Array(field.BinaryName.AsBytes(), out var f64a):
					value = f64a;
					return true;

				case ValueTypeMarker.ArrayDateTime when Buffer.TryGetDateTimeArray(field.BinaryName.AsBytes(), out var dta):
					value = dta;
					return true;

				case ValueTypeMarker.ArrayBoolean when Buffer.TryGetBooleanArray(field.BinaryName.AsBytes(), out var ba):
					value = ba;
					return true;

				case ValueTypeMarker.ArrayString when Buffer.TryGetStringArray(field.BinaryName.AsBytes(), out var sa):
					value = sa;
					return true;

				default:
					Debug.Assert(false);
					throw new NotImplementedException();
			}
		}

		public bool TryGetWrapped(BarbadosIdentifier field, out BarbadosDocument extracted)
		{
			if (field.IsDocument)
			{
				return TryGetDocument(field, out extracted);
			}

			if (Buffer.TryGetBuffer(field.BinaryName.AsBytes(), out var valueBuffer))
			{
				var buffer = new RadixTreeBuffer.Builder()
					.AddBuffer(field, valueBuffer)
					.Build();

				extracted = new BarbadosDocument(ObjectId.Invalid, buffer);
				return true;
			}

			extracted = default!;
			return false;
		}

		public bool TryGetInt8(BarbadosIdentifier field, out sbyte value) => Buffer.TryGetInt8(field.BinaryName.AsBytes(), out value);
		public bool TryGetInt16(BarbadosIdentifier field, out short value) => Buffer.TryGetInt16(field.BinaryName.AsBytes(), out value);
		public bool TryGetInt32(BarbadosIdentifier field, out int value) => Buffer.TryGetInt32(field.BinaryName.AsBytes(), out value);
		public bool TryGetInt64(BarbadosIdentifier field, out long value) => Buffer.TryGetInt64(field.BinaryName.AsBytes(), out value);
		public bool TryGetUInt8(BarbadosIdentifier field, out byte value) => Buffer.TryGetUInt8(field.BinaryName.AsBytes(), out value);
		public bool TryGetUInt16(BarbadosIdentifier field, out ushort value) => Buffer.TryGetUInt16(field.BinaryName.AsBytes(), out value);
		public bool TryGetUInt32(BarbadosIdentifier field, out uint value) => Buffer.TryGetUInt32(field.BinaryName.AsBytes(), out value);
		public bool TryGetUInt64(BarbadosIdentifier field, out ulong value) => Buffer.TryGetUInt64(field.BinaryName.AsBytes(), out value);
		public bool TryGetFloat32(BarbadosIdentifier field, out float value) => Buffer.TryGetFloat32(field.BinaryName.AsBytes(), out value);
		public bool TryGetFloat64(BarbadosIdentifier field, out double value) => Buffer.TryGetFloat64(field.BinaryName.AsBytes(), out value);
		public bool TryGetDateTime(BarbadosIdentifier field, out DateTime value) => Buffer.TryGetDateTime(field.BinaryName.AsBytes(), out value);
		public bool TryGetBoolean(BarbadosIdentifier field, out bool value) => Buffer.TryGetBoolean(field.BinaryName.AsBytes(), out value);
		public bool TryGetString(BarbadosIdentifier field, out string value) => Buffer.TryGetString(field.BinaryName.AsBytes(), out value);
		public bool TryGetInt8Array(BarbadosIdentifier field, out sbyte[] array) => Buffer.TryGetInt8Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetInt16Array(BarbadosIdentifier field, out short[] array) => Buffer.TryGetInt16Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetInt32Array(BarbadosIdentifier field, out int[] array) => Buffer.TryGetInt32Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetInt64Array(BarbadosIdentifier field, out long[] array) => Buffer.TryGetInt64Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetUInt8Array(BarbadosIdentifier field, out byte[] array) => Buffer.TryGetUInt8Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetUInt16Array(BarbadosIdentifier field, out ushort[] array) => Buffer.TryGetUInt16Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetUInt32Array(BarbadosIdentifier field, out uint[] array) => Buffer.TryGetUInt32Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetUInt64Array(BarbadosIdentifier field, out ulong[] array) => Buffer.TryGetUInt64Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetFloat32Array(BarbadosIdentifier field, out float[] array) => Buffer.TryGetFloat32Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetFloat64Array(BarbadosIdentifier field, out double[] array) => Buffer.TryGetFloat64Array(field.BinaryName.AsBytes(), out array);
		public bool TryGetDateTimeArray(BarbadosIdentifier field, out DateTime[] array) => Buffer.TryGetDateTimeArray(field.BinaryName.AsBytes(), out array);
		public bool TryGetBooleanArray(BarbadosIdentifier field, out bool[] array) => Buffer.TryGetBooleanArray(field.BinaryName.AsBytes(), out array);
		public bool TryGetStringArray(BarbadosIdentifier field, out string[] array) => Buffer.TryGetStringArray(field.BinaryName.AsBytes(), out array);
		public bool TryGetInt8FromArray(BarbadosIdentifier field, int index, out sbyte value) => Buffer.TryGetFromInt8Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetInt16FromArray(BarbadosIdentifier field, int index, out short value) => Buffer.TryGetFromInt16Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetInt32FromArray(BarbadosIdentifier field, int index, out int value) => Buffer.TryGetFromInt32Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetInt64FromArray(BarbadosIdentifier field, int index, out long value) => Buffer.TryGetFromInt64Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetUInt8FromArray(BarbadosIdentifier field, int index, out byte value) => Buffer.TryGetFromUInt8Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetUInt16FromArray(BarbadosIdentifier field, int index, out ushort value) => Buffer.TryGetFromUInt16Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetUInt32FromArray(BarbadosIdentifier field, int index, out uint value) => Buffer.TryGetFromUInt32Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetUInt64FromArray(BarbadosIdentifier field, int index, out ulong value) => Buffer.TryGetFromUInt64Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetFloat32FromArray(BarbadosIdentifier field, int index, out float value) => Buffer.TryGetFromFloat32Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetFloat64FromArray(BarbadosIdentifier field, int index, out double value) => Buffer.TryGetFromFloat64Array(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetDateTimeFromArray(BarbadosIdentifier field, int index, out DateTime value) => Buffer.TryGetFromDateTimeArray(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetBooleanFromArray(BarbadosIdentifier field, int index, out bool value) => Buffer.TryGetFromBooleanArray(field.BinaryName.AsBytes(), index, out value);
		public bool TryGetStringFromArray(BarbadosIdentifier field, int index, out string value) => Buffer.TryGetFromStringArray(field.BinaryName.AsBytes(), index, out value);

		public bool TryGetDocument(BarbadosIdentifier field, out BarbadosDocument document)
		{
			if (!field.IsDocument)
			{
				field = field.GetDocumentIdentifier();
			}

			if (Buffer.TryExtract(field.BinaryName.AsBytes(), out var buffer))
			{
				document = new BarbadosDocument(ObjectId.Invalid, buffer);
				return true;
			}

			document = default!;
			return false;
		}

		public bool TryGetDocumentArray(BarbadosIdentifier field, out BarbadosDocument[] documents)
		{
			var sb = new StringBuilder(field);
			if (!field.IsDocument)
			{
				sb.Append(CommonIdentifiers.NestingSeparator);
			}
			sb.Append(0);
			sb.Append(CommonIdentifiers.NestingSeparator);

			BarbadosIdentifier firstItemName = sb.ToString();
			if (!Buffer.PrefixExists(firstItemName.BinaryName.AsBytes()))
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
				sb.Length = field.Identifier.Length + 1;
				sb.Append(count);
				sb.Append(CommonIdentifiers.NestingSeparator);
			}

			documents = [.. l];
			return true;
		}

		public bool TryGetDocumentFromArray(BarbadosIdentifier field, int index, out BarbadosDocument document)
		{
			var sb = new StringBuilder($"{field}{CommonIdentifiers.NestingSeparator}{index}{CommonIdentifiers.NestingSeparator}");
			BarbadosIdentifier itemName = sb.ToString();
			return TryGetDocument(itemName, out document);
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			_toString(this, sb, 0);
			return sb.ToString();
		}
	}
}
