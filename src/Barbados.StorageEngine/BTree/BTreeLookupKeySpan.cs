using System.Diagnostics;

namespace Barbados.StorageEngine.BTree
{
	internal readonly ref struct BTreeLookupKeySpan
	{
		public bool IsTrimmed { get; }

		public BTreeNormalisedValueSpan Separator { get; }

		public BTreeLookupKeySpan(BTreeNormalisedValueSpan separator, bool isTrimmed)
		{
			Debug.Assert(Separator.Bytes.Length <= BTreeInfo.LimitMaxLookupKeyLength);
			Separator = separator;
			IsTrimmed = isTrimmed;
		}
	}
}
