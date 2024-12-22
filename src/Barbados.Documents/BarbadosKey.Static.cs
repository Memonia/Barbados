using System;

using Barbados.Documents.Serialisation;

namespace Barbados.Documents
{
	public partial struct BarbadosKey
	{
		internal static ReadOnlySpan<char> GetRootDocumentPortion(ReadOnlySpan<char> key)
		{
			var index = key.IndexOf(NestingSeparator);
			if (index == -1)
			{
				return key;
			}

			return key[..index];
		}

		internal static ReadOnlySpan<byte> GetRootDocumentPortion(RadixTreePrefixSpan prefix)
		{
			var index = prefix.AsBytes().IndexOf(_nestingSepAsPrefix.AsBytes());
			if (index == -1)
			{
				return prefix.AsBytes();
			}

			return prefix.AsBytes()[..index];
		}
	}
}
