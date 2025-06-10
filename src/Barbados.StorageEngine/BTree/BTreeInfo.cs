using Barbados.StorageEngine.BTree.Pages;
using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.BTree
{
	internal sealed class BTreeInfo
	{
		public const int LimitMinKeysPerPage = 4;
		public const int LimitMaxKeyDataLength = 990;
		public const int LimitMaxLookupKeyLength = 256;
		public const int LimitMaxDataLength = LimitMaxKeyDataLength - LimitMaxLookupKeyLength;
		public const int LimitMaxExternalLookupKeyLength = LimitMaxLookupKeyLength - BTreeContext.WorstCaseOverheadPerLookupKey;

#pragma warning disable IDE0051 // Remove unused private members

		private const uint _ASSERT_MIN_ITEM_COUNT_PER_BTREE_NODE =
			(BTreePage.PayloadLength - BTreePage.WorstCaseFixedLengthOverheadPerEntry * LimitMinKeysPerPage) / LimitMaxKeyDataLength
			>= LimitMinKeysPerPage ? 0 : -1;

		private const uint _ASSERT_MIN_ITEM_COUNT_PER_BTREE_LEAF =
			(BTreeLeafPage.PayloadLength - BTreeLeafPage.WorstCaseFixedLengthOverheadPerEntry * LimitMinKeysPerPage) / LimitMaxKeyDataLength
			>= LimitMinKeysPerPage ? 0 : -1;

		private const uint _ASSERT_MAX_DATA_LENGTH_POSITIVE = LimitMaxDataLength > 0 ? 0 : -1;
		private const uint _ASSERT_MAX_EXTERNAL_LOOKUP_KEY_LENGTH_POSITIVE = LimitMaxExternalLookupKeyLength > 0 ? 0 : -1;

#pragma warning restore IDE0051 // Remove unused private members

		public int MaxKeyDataLength { get; } = LimitMaxKeyDataLength;
		public int MaxLookupKeyLength { get; } = LimitMaxExternalLookupKeyLength;
		public int MaxDataLength { get; } = LimitMaxDataLength;
		public long MaxSamePrefixKeyCount { get; } = long.MaxValue;
		public PageHandle RootHandle { get; }

		public BTreeInfo(PageHandle handle)
		{
			RootHandle = handle;
		}
	}
}
