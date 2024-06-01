using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine
{
	public interface IReadOnlyBTreeIndex
	{
		BarbadosIdentifier Name { get; }
		BarbadosIdentifier Field { get; }

		ICursor<ObjectId> Find(BarbadosDocument condition);
		ICursor<ObjectId> FindExact<T>(T searchValue);
	}
}
