using System;

using Barbados.Documents.RadixTree;

namespace Barbados.Documents
{
	public partial struct BarbadosKey
	{
		internal static ReadOnlySpan<char> GetFirstNestingLevel(ReadOnlySpan<char> prefix)
		{
			var i = prefix.IndexOf(NestingSeparator);
			if (i < 0)
			{
				return prefix;
			}

			return prefix[..i];
		}

		internal static RadixTreePrefixSpan GetFirstNestingLevel(RadixTreePrefixSpan prefix)
		{
			var i = prefix.AsBytes().IndexOf(NestingSeparatorAsPrefix.AsBytes());
			if (i < 0)
			{
				return prefix;
			}

			return prefix[..i];
		}
	}
}
