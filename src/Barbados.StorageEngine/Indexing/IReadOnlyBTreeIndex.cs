using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine.Indexing
{
	public interface IReadOnlyBTreeIndex
	{
		ObjectId CollectionId { get; }
		BarbadosIdentifier IndexField { get; }

		ICursor<ObjectId> Find(BarbadosDocument condition);
		ICursor<ObjectId> FindExact<T>(T searchValue);
	}
}
