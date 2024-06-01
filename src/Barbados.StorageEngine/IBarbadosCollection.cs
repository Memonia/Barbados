using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine
{
	public interface IBarbadosCollection : IBarbadosReadOnlyCollection
	{
		ObjectId Insert(BarbadosDocument document);

		bool TryUpdate(ObjectId id, BarbadosDocument document);
		bool TryRemove(ObjectId Id);

		void Update(ObjectId id, BarbadosDocument document);
		void Remove(ObjectId id);
	}
}
