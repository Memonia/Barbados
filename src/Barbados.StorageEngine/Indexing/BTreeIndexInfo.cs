using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed class BTreeIndexInfo
	{
		public required int KeyMaxLength { get; init; }

		public required PageHandle RootPageHandle { get; init; }
	}
}
