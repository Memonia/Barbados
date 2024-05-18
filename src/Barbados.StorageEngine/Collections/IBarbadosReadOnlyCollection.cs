using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine.Collections
{
	public interface IBarbadosReadOnlyCollection
	{
		BarbadosIdentifier Name { get; }

		IBarbadosController Controller { get; }

		bool TryRead(ObjectId id, out BarbadosDocument document);
		bool TryRead(ObjectId id, ValueSelector selector, out BarbadosDocument document);

		void Read(ObjectId id, out BarbadosDocument document);
		void Read(ObjectId id, ValueSelector selector, out BarbadosDocument document);

		ICursor<BarbadosDocument> GetCursor();
		ICursor<BarbadosDocument> GetCursor(ValueSelector selector);
	}
}
