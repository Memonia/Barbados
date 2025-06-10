using Barbados.Documents;

namespace Barbados.StorageEngine.Collections
{
	public interface IBarbadosCollection : IReadOnlyBarbadosCollection
	{
		BarbadosDocument InsertWithAutomaticId(BarbadosDocument.Builder builder);

		void Insert(BarbadosDocument document);
		void Update(BarbadosDocument document);
		void Remove(BarbadosDocument document);

		bool TryInsert(BarbadosDocument document);
		bool TryUpdate(BarbadosDocument document);
		bool TryRemove(BarbadosDocument document);
	}
}
