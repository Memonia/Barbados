using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Paging
{
	internal interface IBTreeIndexLeaf<T> : ITwoWayChainPage where T : IBTreeIndexLeaf<T>
	{
		public bool IsUnderflowed { get; }

		public bool TryReadLowest(out BTreeIndexKey key);
		public bool TryReadHighest(out BTreeIndexKey key);

		public void Flush(T to, bool fromHighest);
		public void Spill(T to, bool fromHighest);
	}
}
