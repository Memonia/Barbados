using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine.Indexing
{
	public interface IBTreeIndexLookup
	{
		BarbadosIdentifier Name { get; }
		BarbadosIdentifier IndexedField { get; }

		ICursor<ObjectId> Find(BarbadosDocument condition);
		ICursor<ObjectId> FindExact<T>(T searchValue);
	}
}
