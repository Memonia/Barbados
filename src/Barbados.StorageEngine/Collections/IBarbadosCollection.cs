using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine.Collections
{
	public interface IBarbadosCollection : IReadOnlyBarbadosCollection
	{
		bool TryUpdate(ObjectId id, BarbadosDocument document);
		bool TryRemove(ObjectId Id);

		void Update(ObjectId id, BarbadosDocument document);
		void Remove(ObjectId id);

		ObjectId Insert(BarbadosDocument document);
	}
}
