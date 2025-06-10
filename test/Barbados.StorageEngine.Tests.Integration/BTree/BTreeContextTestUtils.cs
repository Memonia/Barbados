using System;

using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Storage;

namespace Barbados.StorageEngine.Tests.Integration.BTree
{
	internal static class BTreeContextTestUtils
	{
		public static BTreeNormalisedValue CreateStringKeyFrom(int key, int keyLength)
		{
			return CreateStringKeyFrom(key, keyLength, isKeyExternal: false);
		}

		public static BTreeNormalisedValue CreateStringKeyFrom(int key, int keyLength, bool isKeyExternal)
		{
			var str = Convert.ToString(key).PadLeft(keyLength, '0');
			var diff = BTreeNormalisedValue.GetLength(str, isKeyExternal) - keyLength;
			return BTreeNormalisedValue.Create(str[diff..], isKeyExternal);
		}

		public static void FillDataBytes(int value, in byte[] bytes)
		{
			Array.Fill(bytes, (byte)'d');
			if (bytes.Length < 4)
			{
				for (int i = 0; i < bytes.Length; ++i)
				{
					bytes[i] = (byte)(value >> i * 8 & 0xFF);
				}
			}

			else
			{
				HelpWrite.AsInt32(bytes, value);
			}
		}

		public static byte[] CreateDataBytes(int value, int length)
		{
			var bytes = new byte[length];
			FillDataBytes(value, in bytes);
			return bytes;
		}
	}
}
