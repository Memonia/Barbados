using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine.Indexing
{
	public interface IBTreeIndexLookup
	{
		BarbadosIdentifier Name { get; }
		BarbadosIdentifier Collection { get; }
		BarbadosIdentifier IndexedField { get; }

		IBarbadosController Controller { get; }

		ICursor<ObjectId> Find(BarbadosDocument condition);
		ICursor<ObjectId> FindExact<T>(T searchValue);
	}
}
