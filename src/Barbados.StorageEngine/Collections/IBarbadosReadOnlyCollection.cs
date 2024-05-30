using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Collections
{
	public interface IBarbadosReadOnlyCollection
	{
		BarbadosIdentifier Name { get; }
		
		bool TryRead(ObjectId id, out BarbadosDocument document);
		bool TryRead(ObjectId id, ValueSelector selector, out BarbadosDocument document);
		bool TryGetBTreeIndexLookup(BarbadosIdentifier field, out IBTreeIndexLookup lookup);

		void Read(ObjectId id, out BarbadosDocument document);
		void Read(ObjectId id, ValueSelector selector, out BarbadosDocument document); 

		ICursor<BarbadosDocument> GetCursor();
		ICursor<BarbadosDocument> GetCursor(ValueSelector selector);
	}
}
