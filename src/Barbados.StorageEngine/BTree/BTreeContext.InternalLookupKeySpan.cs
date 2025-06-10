using System.Diagnostics;

namespace Barbados.StorageEngine.BTree
{
	internal partial class BTreeContext
	{
		private readonly ref struct InternalLookupKeySpan
		{
			public bool IsChunkKey { get; }

			public BTreeNormalisedValueSpan Separator { get; }

			public InternalLookupKeySpan(BTreeNormalisedValueSpan separator, bool isChunkKey)
			{
				Debug.Assert(Separator.Bytes.Length <= BTreeInfo.LimitMaxLookupKeyLength);
				Separator = separator;
				IsChunkKey = isChunkKey;
			}
		}
	}
}
