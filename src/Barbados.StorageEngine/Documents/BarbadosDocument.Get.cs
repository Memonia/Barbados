using System;

using Barbados.StorageEngine.Documents.Exceptions;

namespace Barbados.StorageEngine.Documents
{
	public partial class BarbadosDocument
	{
		public sbyte GetInt8(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetInt8(field, out var value), value, field
			);
		}

		public short GetInt16(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetInt16(field, out var value), value, field
			);
		}

		public int GetInt32(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetInt32(field, out var value), value, field
			);
		}

		public long GetInt64(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetInt64(field, out var value), value, field
			);
		}

		public byte GetUInt8(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt8(field, out var value), value, field
			);
		}

		public ushort GetUInt16(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt16(field, out var value), value, field
			);
		}

		public uint GetUInt32(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt32(field, out var value), value, field
			);
		}

		public ulong GetUInt64(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt64(field, out var value), value, field
			);
		}

		public float GetFloat32(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetFloat32(field, out var value), value, field
			);
		}

		public double GetFloat64(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetFloat64(field, out var value), value, field
			);
		}

		public DateTime GetDateTime(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetDateTime(field, out var value), value, field
			);
		}

		public bool GetBoolean(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetBoolean(field, out var value), value, field
			);
		}

		public string GetString(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetString(field, out var value), value, field
			);
		}

		public BarbadosDocument GetDocument(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetDocument(field, out var value), value, field
			);
		}

		public sbyte[] GetInt8Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetInt8Array(field, out var value), value, field
			);
		}

		public short[] GetInt16Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetInt16Array(field, out var value), value, field
			);
		}

		public int[] GetInt32Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetInt32Array(field, out var value), value, field
			);
		}

		public long[] GetInt64Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetInt64Array(field, out var value), value, field
			);
		}

		public byte[] GetUInt8Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt8Array(field, out var value), value, field
			);
		}

		public ushort[] GetUInt16Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt16Array(field, out var value), value, field
			);
		}

		public uint[] GetUInt32Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt32Array(field, out var value), value, field
			);
		}

		public ulong[] GetUInt64Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt64Array(field, out var value), value, field
			);
		}

		public float[] GetFloat32Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetFloat32Array(field, out var value), value, field
			);
		}

		public double[] GetFloat64Array(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetFloat64Array(field, out var value), value, field
			);
		}

		public DateTime[] GetDateTimeArray(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetDateTimeArray(field, out var value), value, field
			);
		}

		public bool[] GetBooleanArray(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetBooleanArray(field, out var value), value, field
			);
		}

		public string[] GetStringArray(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetStringArray(field, out var value), value, field
			);
		}

		public BarbadosDocument[] GetDocumentArray(BarbadosIdentifier field)
		{
			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(
				TryGetDocumentArray(field, out var value), value, field
			);
		}

		public sbyte GetInt8FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetInt8FromArray(field, index, out var value), index, value, field
			);
		}

		public short GetInt16FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetInt16FromArray(field, index, out var value), index, value, field
			);
		}

		public int GetInt32FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetInt32FromArray(field, index, out var value), index, value, field
			);
		}

		public long GetInt64FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetInt64FromArray(field, index, out var value), index, value, field
			);
		}

		public byte GetUInt8FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt8FromArray(field, index, out var value), index, value, field
			);
		}

		public ushort GetUInt16FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt16FromArray(field, index, out var value), index, value, field
			);
		}

		public uint GetUInt32FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt32FromArray(field, index, out var value), index, value, field
			);
		}

		public ulong GetUInt64FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetUInt64FromArray(field, index, out var value), index, value, field
			);
		}

		public float GetFloat32FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetFloat32FromArray(field, index, out var value), index, value, field
			);
		}

		public double GetFloat64FromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetFloat64FromArray(field, index, out var value), index, value, field
			);
		}

		public DateTime GetDateTimeFromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetDateTimeFromArray(field, index, out var value), index, value, field
			);
		}

		public bool GetBooleanFromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetBooleanFromArray(field, index, out var value), index, value, field
			);
		}

		public string GetStringFromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetStringFromArray(field, index, out var value), index, value, field
			);
		}

		public BarbadosDocument GetDocumentFromArray(BarbadosIdentifier field, int index)
		{
			return _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue(
				TryGetDocumentFromArray(field, index, out var value), index, value, field
			);
		}

		private T _throwIndexOutOfBoundsOrFieldOfTypeNotFoundOrReturnValue<T>(bool result, int index, T value, BarbadosIdentifier field)
		{
			if (result)
			{
				return value;
			}

			if (TryGetArrayCount(field, out var count) && count <= index)
			{
				throw new IndexOutOfRangeException();
			}

			return BarbadosDocumentInvalidOperationException.ThrowFieldOfTypeNotFoundOrReturnValue(result, value, field);
		}
	}
}
