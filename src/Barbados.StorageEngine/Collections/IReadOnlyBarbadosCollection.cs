using Barbados.Documents;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Collections
{
	public interface IReadOnlyBarbadosCollection
	{
		ObjectId Id { get; }
		BarbadosDbObjectName Name { get; }

		bool TryGetBTreeIndex(string field, out IReadOnlyBTreeIndex index);

		bool TryRead(ObjectId id, out BarbadosDocument document);
		bool TryRead(ObjectId id, BarbadosKeySelector selector, out BarbadosDocument document);

		BarbadosDocument Read(ObjectId id);
		BarbadosDocument Read(ObjectId id, BarbadosKeySelector selector);

		ICursor<BarbadosDocument> GetCursor();
		ICursor<BarbadosDocument> GetCursor(BarbadosKeySelector selector);
	}
}
