using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Paging
{
	internal interface IBTreeIndexLeaf<T> : ITwoWayChainPage where T : IBTreeIndexLeaf<T>
	{
		public bool IsUnderflowed { get; }

		public bool TryReadLowest(out NormalisedValueSpan key);
		public bool TryReadHighest(out NormalisedValueSpan key);

		public void Flush(T to, bool fromHighest);
		public void Spill(T to, bool fromHighest);
	}
}
