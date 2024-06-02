using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine
{
	public interface IReadOnlyBarbadosCollection
	{
		BarbadosIdentifier Name { get; }

		bool TryRead(ObjectId id, out BarbadosDocument document);
		bool TryRead(ObjectId id, ValueSelector selector, out BarbadosDocument document);
		bool TryGetBTreeIndexLookup(BarbadosIdentifier field, out IReadOnlyBTreeIndex lookup);

		void Read(ObjectId id, out BarbadosDocument document);
		void Read(ObjectId id, ValueSelector selector, out BarbadosDocument document);

		ICursor<BarbadosDocument> GetCursor();
		ICursor<BarbadosDocument> GetCursor(ValueSelector selector);
	}
}
