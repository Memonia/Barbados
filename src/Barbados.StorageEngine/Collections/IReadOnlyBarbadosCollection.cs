using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Collections
{
	public interface IReadOnlyBarbadosCollection
	{
		ObjectId Id { get; }
		BarbadosIdentifier Name { get; }

		bool TryGetBTreeIndex(string field, out IReadOnlyBTreeIndex index);

		bool TryRead(ObjectId id, out BarbadosDocument document);
		bool TryRead(ObjectId id, ValueSelector selector, out BarbadosDocument document);

		BarbadosDocument Read(ObjectId id);
		BarbadosDocument Read(ObjectId id, ValueSelector selector);

		ICursor<BarbadosDocument> GetCursor();
		ICursor<BarbadosDocument> GetCursor(ValueSelector selector);
	}
}
