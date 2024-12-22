using System;

using Barbados.Documents.Exceptions;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public object Get(BarbadosKey field)
		{
			if (TryGet(field, out var value))
			{
				return value;
			}

			throw new InvalidOperationException($"Document does not contain field '{field}'");
		}

		public sbyte GetInt8(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetInt8(field, out var value), value, field
			);
		}

		public short GetInt16(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetInt16(field, out var value), value, field
			);
		}

		public int GetInt32(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetInt32(field, out var value), value, field
			);
		}

		public long GetInt64(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetInt64(field, out var value), value, field
			);
		}

		public byte GetUInt8(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetUInt8(field, out var value), value, field
			);
		}

		public ushort GetUInt16(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetUInt16(field, out var value), value, field
			);
		}

		public uint GetUInt32(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetUInt32(field, out var value), value, field
			);
		}

		public ulong GetUInt64(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetUInt64(field, out var value), value, field
			);
		}

		public float GetFloat32(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetFloat32(field, out var value), value, field
			);
		}

		public double GetFloat64(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetFloat64(field, out var value), value, field
			);
		}

		public DateTime GetDateTime(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetDateTime(field, out var value), value, field
			);
		}

		public bool GetBoolean(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetBoolean(field, out var value), value, field
			);
		}

		public string GetString(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetString(field, out var value), value, field
			);
		}

		public BarbadosDocument GetDocument(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetDocument(field, out var value), value, field
			);
		}

		public sbyte[] GetInt8Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetInt8Array(field, out var value), value, field
			);
		}

		public short[] GetInt16Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetInt16Array(field, out var value), value, field
			);
		}

		public int[] GetInt32Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetInt32Array(field, out var value), value, field
			);
		}

		public long[] GetInt64Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetInt64Array(field, out var value), value, field
			);
		}

		public byte[] GetUInt8Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetUInt8Array(field, out var value), value, field
			);
		}

		public ushort[] GetUInt16Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetUInt16Array(field, out var value), value, field
			);
		}

		public uint[] GetUInt32Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetUInt32Array(field, out var value), value, field
			);
		}

		public ulong[] GetUInt64Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetUInt64Array(field, out var value), value, field
			);
		}

		public float[] GetFloat32Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetFloat32Array(field, out var value), value, field
			);
		}

		public double[] GetFloat64Array(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetFloat64Array(field, out var value), value, field
			);
		}

		public DateTime[] GetDateTimeArray(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetDateTimeArray(field, out var value), value, field
			);
		}

		public bool[] GetBooleanArray(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetBooleanArray(field, out var value), value, field
			);
		}

		public string[] GetStringArray(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetStringArray(field, out var value), value, field
			);
		}

		public BarbadosDocument[] GetDocumentArray(BarbadosKey field)
		{
			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(
				TryGetDocumentArray(field, out var value), value, field
			);
		}

		public sbyte GetInt8FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetInt8FromArray(field, index, out var value), index, value, field
			);
		}

		public short GetInt16FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetInt16FromArray(field, index, out var value), index, value, field
			);
		}

		public int GetInt32FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetInt32FromArray(field, index, out var value), index, value, field
			);
		}

		public long GetInt64FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetInt64FromArray(field, index, out var value), index, value, field
			);
		}

		public byte GetUInt8FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt8FromArray(field, index, out var value), index, value, field
			);
		}

		public ushort GetUInt16FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt16FromArray(field, index, out var value), index, value, field
			);
		}

		public uint GetUInt32FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt32FromArray(field, index, out var value), index, value, field
			);
		}

		public ulong GetUInt64FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt64FromArray(field, index, out var value), index, value, field
			);
		}

		public float GetFloat32FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetFloat32FromArray(field, index, out var value), index, value, field
			);
		}

		public double GetFloat64FromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetFloat64FromArray(field, index, out var value), index, value, field
			);
		}

		public DateTime GetDateTimeFromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetDateTimeFromArray(field, index, out var value), index, value, field
			);
		}

		public bool GetBooleanFromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetBooleanFromArray(field, index, out var value), index, value, field
			);
		}

		public string GetStringFromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetStringFromArray(field, index, out var value), index, value, field
			);
		}

		public BarbadosDocument GetDocumentFromArray(BarbadosKey field, int index)
		{
			return _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue(
				TryGetDocumentFromArray(field, index, out var value), index, value, field
			);
		}

		private T _throwIndexOutOfRangeOrFieldOfTypeNotFoundOrReturnValue<T>(bool result, int index, T value, BarbadosKey field)
		{
			if (result)
			{
				return value;
			}

			if (TryGetArrayCount(field, out var count) && count <= index)
			{
				throw new IndexOutOfRangeException();
			}

			return BarbadosDocumentException.ThrowElementOfTypeNotFoundOrReturnValue(result, value, field);
		}
	}
}
