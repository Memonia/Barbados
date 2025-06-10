using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class CollectionInfo
	{
		public required BTreeInfo BTreeInfo { get; init; }
		public required ObjectId CollectionId { get; init; }
		public required AutomaticIdGeneratorMode AutomaticIdGeneratorMode { get; init; }
	}
}
