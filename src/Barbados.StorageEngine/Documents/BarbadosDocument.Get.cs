using System;

using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine.Documents
{
	public partial class BarbadosDocument
	{
		public sbyte GetInt8(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetInt8(field, out var value), value, field, typeof(sbyte)
			);
		}

		public short GetInt16(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetInt16(field, out var value), value, field, typeof(short)
			);
		}

		public int GetInt32(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetInt32(field, out var value), value, field, typeof(int)
			);
		}

		public long GetInt64(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetInt64(field, out var value), value, field, typeof(long)
			);
		}

		public byte GetUInt8(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetUInt8(field, out var value), value, field, typeof(byte)
			);
		}

		public ushort GetUInt16(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetUInt16(field, out var value), value, field, typeof(ushort)
			);
		}

		public uint GetUInt32(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetUInt32(field, out var value), value, field, typeof(uint)
			);
		}

		public ulong GetUInt64(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetUInt64(field, out var value), value, field, typeof(ulong)
			);
		}

		public float GetFloat32(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetFloat32(field, out var value), value, field, typeof(float)
			);
		}

		public double GetFloat64(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetFloat64(field, out var value), value, field, typeof(double)
			);
		}

		public DateTime GetDateTime(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetDateTime(field, out var value), value, field, typeof(DateTime)
			);
		}

		public bool GetBoolean(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetBoolean(field, out var value), value, field, typeof(bool)
			);
		}

		public string GetString(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetString(field, out var value), value, field, typeof(string)
			);
		}

		public sbyte[] GetInt8Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetInt8Array(field, out var value), value, field, typeof(sbyte[])
			);
		}

		public short[] GetInt16Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetInt16Array(field, out var value), value, field, typeof(short[])
			);
		}

		public int[] GetInt32Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetInt32Array(field, out var value), value, field, typeof(int[])
			);
		}

		public long[] GetInt64Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetInt64Array(field, out var value), value, field, typeof(long[])
			);
		}

		public byte[] GetUInt8Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetUInt8Array(field, out var value), value, field, typeof(byte[])
			);
		}

		public ushort[] GetUInt16Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetUInt16Array(field, out var value), value, field, typeof(ushort[])
			);
		}

		public uint[] GetUInt32Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetUInt32Array(field, out var value), value, field, typeof(uint[])
			);
		}

		public ulong[] GetUInt64Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetUInt64Array(field, out var value), value, field, typeof(ulong[])
			);
		}

		public float[] GetFloat32Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetFloat32Array(field, out var value), value, field, typeof(float[])
			);
		}

		public double[] GetFloat64Array(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetFloat64Array(field, out var value), value, field, typeof(double[])
			);
		}

		public DateTime[] GetDateTimeArray(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetDateTimeArray(field, out var value), value, field, typeof(DateTime[])
			);
		}

		public bool[] GetBooleanArray(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetBooleanArray(field, out var value), value, field, typeof(bool[])
			);
		}

		public string[] GetStringArray(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetStringArray(field, out var value), value, field, typeof(string[])
			);
		}

		public BarbadosDocument GetDocument(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetDocument(field, out var value), value, field, typeof(BarbadosDocument)
			);
		}

		public BarbadosDocument[] GetDocumentArray(BarbadosIdentifier field)
		{
			return BarbadosInvalidOperationException.ThrowFieldOfTypeNotFoundIfResultFalse(
				TryGetDocumentArray(field, out var value), value, field, typeof(BarbadosDocument[])
			);
		}
	}
}
