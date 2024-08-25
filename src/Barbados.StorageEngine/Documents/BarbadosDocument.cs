using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Documents
{
	public sealed partial class BarbadosDocument
	{
		public ObjectId Id { get; }

		internal ObjectBuffer Buffer { get; }

		internal BarbadosDocument(ObjectId id, ObjectBuffer buffer)
		{
			Id = id;
			Buffer = buffer;
		}

		public int Count() => Buffer.Count();

		public List<BarbadosIdentifier> GetFields()
		{
			var fields = new List<BarbadosIdentifier>();
			var groups = new List<string>();
			var e = Buffer.GetNameEnumerator();
			while (e.TryGetNext(out _, out var name))
			{
				BarbadosIdentifier n = name;

				// Is current field a part of any group?
				var splitIndices = n.GetSplitIndices();
				BarbadosIdentifier group;
				if (splitIndices.MoveNext() && splitIndices.Current < n.Identifier.Length)
				{
					group = n.Identifier[..(splitIndices.Current + 1)];
					if (!groups.Contains((string)group, StringComparer.Ordinal))
					{
						fields.Add(group.GetGroupName());
						groups.Add(group);
					}
				}

				else
				{
					fields.Add(n);
				}
			}

			return fields;
		}

		public List<BarbadosIdentifier> GetFieldsDeep()
		{
			var fields = new List<BarbadosIdentifier>();
			var e = Buffer.GetNameEnumerator();
			while (e.TryGetNext(out _, out var name))
			{
				fields.Add(name);
			}

			return fields;
		}

		public bool HasGroup(BarbadosIdentifier group)
		{
			return Buffer.PrefixExists(group.BinaryName.AsSpan());
		}

		public bool HasField(BarbadosIdentifier field)
		{
			return Buffer.ValueExists(field.BinaryName.AsSpan());
		}

		public bool TryGet(BarbadosIdentifier field, out object value)
		{
			if (!Buffer.ValueExists(field.BinaryName.AsSpan(), out var marker, out var isArray))
			{
				value = default!;
				return false;
			}

			bool retrieved;
			if (isArray)
			{
				switch (marker)
				{
					case ValueTypeMarker.Int8:
						retrieved = Buffer.TryGetInt8Array(field.BinaryName.AsSpan(), out var i8);
						value = i8;
						break;

					case ValueTypeMarker.Int16:
						retrieved = Buffer.TryGetInt16Array(field.BinaryName.AsSpan(), out var i16);
						value = i16;
						break;

					case ValueTypeMarker.Int32:
						retrieved = Buffer.TryGetInt32Array(field.BinaryName.AsSpan(), out var i32);
						value = i32;
						break;

					case ValueTypeMarker.Int64:
						retrieved = Buffer.TryGetInt64Array(field.BinaryName.AsSpan(), out var i64);
						value = i64;
						break;

					case ValueTypeMarker.UInt8:
						retrieved = Buffer.TryGetUInt8Array(field.BinaryName.AsSpan(), out var u8);
						value = u8;
						break;

					case ValueTypeMarker.UInt16:
						retrieved = Buffer.TryGetUInt16Array(field.BinaryName.AsSpan(), out var u16);
						value = u16;
						break;

					case ValueTypeMarker.UInt32:
						retrieved = Buffer.TryGetUInt32Array(field.BinaryName.AsSpan(), out var u32);
						value = u32;
						break;

					case ValueTypeMarker.UInt64:
						retrieved = Buffer.TryGetUInt64Array(field.BinaryName.AsSpan(), out var u64);
						value = u64;
						break;

					case ValueTypeMarker.Float32:
						retrieved = Buffer.TryGetFloat32Array(field.BinaryName.AsSpan(), out var f32);
						value = f32;
						break;

					case ValueTypeMarker.Float64:
						retrieved = Buffer.TryGetFloat64Array(field.BinaryName.AsSpan(), out var f64);
						value = f64;
						break;

					case ValueTypeMarker.Boolean:
						retrieved = Buffer.TryGetBooleanArray(field.BinaryName.AsSpan(), out var b);
						value = b;
						break;

					case ValueTypeMarker.DateTime:
						retrieved = Buffer.TryGetDateTimeArray(field.BinaryName.AsSpan(), out var dt);
						value = dt;
						break;

					case ValueTypeMarker.String:
						retrieved = Buffer.TryGetStringArray(field.BinaryName.AsSpan(), out var str);
						value = str;
						break;

					default:
						throw new NotImplementedException();
				}
			}

			else
			{
				switch (marker)
				{
					case ValueTypeMarker.Int8:
						retrieved = Buffer.TryGetInt8(field.BinaryName.AsSpan(), out var i8);
						value = i8;
						break;

					case ValueTypeMarker.Int16:
						retrieved = Buffer.TryGetInt16(field.BinaryName.AsSpan(), out var i16);
						value = i16;
						break;

					case ValueTypeMarker.Int32:
						retrieved = Buffer.TryGetInt32(field.BinaryName.AsSpan(), out var i32);
						value = i32;
						break;

					case ValueTypeMarker.Int64:
						retrieved = Buffer.TryGetInt64(field.BinaryName.AsSpan(), out var i64);
						value = i64;
						break;

					case ValueTypeMarker.UInt8:
						retrieved = Buffer.TryGetUInt8(field.BinaryName.AsSpan(), out var u8);
						value = u8;
						break;

					case ValueTypeMarker.UInt16:
						retrieved = Buffer.TryGetUInt16(field.BinaryName.AsSpan(), out var u16);
						value = u16;
						break;

					case ValueTypeMarker.UInt32:
						retrieved = Buffer.TryGetUInt32(field.BinaryName.AsSpan(), out var u32);
						value = u32;
						break;

					case ValueTypeMarker.UInt64:
						retrieved = Buffer.TryGetUInt64(field.BinaryName.AsSpan(), out var u64);
						value = u64;
						break;

					case ValueTypeMarker.Float32:
						retrieved = Buffer.TryGetFloat32(field.BinaryName.AsSpan(), out var f32);
						value = f32;
						break;

					case ValueTypeMarker.Float64:
						retrieved = Buffer.TryGetFloat64(field.BinaryName.AsSpan(), out var f64);
						value = f64;
						break;

					case ValueTypeMarker.Boolean:
						retrieved = Buffer.TryGetBoolean(field.BinaryName.AsSpan(), out var b);
						value = b;
						break;

					case ValueTypeMarker.DateTime:
						retrieved = Buffer.TryGetDateTime(field.BinaryName.AsSpan(), out var dt);
						value = dt;
						break;

					case ValueTypeMarker.String:
						retrieved = Buffer.TryGetString(field.BinaryName.AsSpan(), out var str);
						value = str;
						break;

					default:
						throw new NotImplementedException();
				}
			}

			Debug.Assert(retrieved);
			return retrieved;
		}

		public bool TryGetWrapped(BarbadosIdentifier field, out BarbadosDocument extracted)
		{
			if (field.IsGroup)
			{
				return TryGetDocument(field, out extracted);
			}

			if (Buffer.TryGetBuffer(field.BinaryName.AsSpan(), out var valueBuffer))
			{
				var buffer = new ObjectBuffer.Builder()
					.AddBuffer(field, valueBuffer)
					.Build();

				extracted = new BarbadosDocument(ObjectId.Invalid, buffer);
				return true;
			}

			extracted = default!;
			return false;
		}

		public bool TryGetInt8(BarbadosIdentifier field, out sbyte value) => Buffer.TryGetInt8(field.BinaryName.AsSpan(), out value);
		public bool TryGetInt16(BarbadosIdentifier field, out short value) => Buffer.TryGetInt16(field.BinaryName.AsSpan(), out value);
		public bool TryGetInt32(BarbadosIdentifier field, out int value) => Buffer.TryGetInt32(field.BinaryName.AsSpan(), out value);
		public bool TryGetInt64(BarbadosIdentifier field, out long value) => Buffer.TryGetInt64(field.BinaryName.AsSpan(), out value);
		public bool TryGetUInt8(BarbadosIdentifier field, out byte value) => Buffer.TryGetUInt8(field.BinaryName.AsSpan(), out value);
		public bool TryGetUInt16(BarbadosIdentifier field, out ushort value) => Buffer.TryGetUInt16(field.BinaryName.AsSpan(), out value);
		public bool TryGetUInt32(BarbadosIdentifier field, out uint value) => Buffer.TryGetUInt32(field.BinaryName.AsSpan(), out value);
		public bool TryGetUInt64(BarbadosIdentifier field, out ulong value) => Buffer.TryGetUInt64(field.BinaryName.AsSpan(), out value);
		public bool TryGetFloat32(BarbadosIdentifier field, out float value) => Buffer.TryGetFloat32(field.BinaryName.AsSpan(), out value);
		public bool TryGetFloat64(BarbadosIdentifier field, out double value) => Buffer.TryGetFloat64(field.BinaryName.AsSpan(), out value);
		public bool TryGetDateTime(BarbadosIdentifier field, out DateTime value) => Buffer.TryGetDateTime(field.BinaryName.AsSpan(), out value);
		public bool TryGetBoolean(BarbadosIdentifier field, out bool value) => Buffer.TryGetBoolean(field.BinaryName.AsSpan(), out value);
		public bool TryGetString(BarbadosIdentifier field, out string value) => Buffer.TryGetString(field.BinaryName.AsSpan(), out value);
		public bool TryGetInt8Array(BarbadosIdentifier field, out sbyte[] array) => Buffer.TryGetInt8Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetInt16Array(BarbadosIdentifier field, out short[] array) => Buffer.TryGetInt16Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetInt32Array(BarbadosIdentifier field, out int[] array) => Buffer.TryGetInt32Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetInt64Array(BarbadosIdentifier field, out long[] array) => Buffer.TryGetInt64Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetUInt8Array(BarbadosIdentifier field, out byte[] array) => Buffer.TryGetUInt8Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetUInt16Array(BarbadosIdentifier field, out ushort[] array) => Buffer.TryGetUInt16Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetUInt32Array(BarbadosIdentifier field, out uint[] array) => Buffer.TryGetUInt32Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetUInt64Array(BarbadosIdentifier field, out ulong[] array) => Buffer.TryGetUInt64Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetFloat32Array(BarbadosIdentifier field, out float[] array) => Buffer.TryGetFloat32Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetFloat64Array(BarbadosIdentifier field, out double[] array) => Buffer.TryGetFloat64Array(field.BinaryName.AsSpan(), out array);
		public bool TryGetDateTimeArray(BarbadosIdentifier field, out DateTime[] array) => Buffer.TryGetDateTimeArray(field.BinaryName.AsSpan(), out array);
		public bool TryGetBooleanArray(BarbadosIdentifier field, out bool[] array) => Buffer.TryGetBooleanArray(field.BinaryName.AsSpan(), out array);
		public bool TryGetStringArray(BarbadosIdentifier field, out string[] array) => Buffer.TryGetStringArray(field.BinaryName.AsSpan(), out array);
	
		public bool TryGetDocument(BarbadosIdentifier field, out BarbadosDocument document)
		{
			if (!field.IsGroup)
			{
				field = field.GetGroupIdentifier();
			}

			if (Buffer.TryCollect(field.BinaryName.AsSpan(), truncateNames: true, out var buffer))
			{
				document = new BarbadosDocument(ObjectId.Invalid, buffer);
				return true;
			}

			document = default!;
			return false;
		}
		
		public bool TryGetDocumentArray(BarbadosIdentifier field, out BarbadosDocument[] documents)
		{
			var sb = new StringBuilder($"{field}.0");
			BarbadosIdentifier firstItemName = sb.ToString();
			if (!Buffer.PrefixExists(firstItemName.BinaryName.AsSpan()))
			{
				documents = default!;
				return false;
			}

			var l = new List<BarbadosDocument>();
			int count = 0;
			do
			{
				if (!TryGetDocument(sb.ToString(), out var document))
				{
					break;
				}

				l.Add(document);
				count += 1;
				sb.Length = field.Identifier.Length + 1;
				sb.Append(count);

			} while (true);

			documents = [.. l];
			return true;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			_toString(this, sb, 0);
			return sb.ToString();
		}
	}
}
