namespace Barbados.StorageEngine
{
	public interface IBarbadosController
	{
		bool TryGetCollection(BarbadosIdentifier name, out IBarbadosCollection collection);
		bool TryGetReadOnlyCollection(BarbadosIdentifier name, out IReadOnlyBarbadosCollection collection);
		IBarbadosCollection GetCollection(BarbadosIdentifier name);
		IReadOnlyBarbadosCollection GetReadOnlyCollection(BarbadosIdentifier name);

		bool TryGetIndex(BarbadosIdentifier collection, BarbadosIdentifier field, out IReadOnlyBTreeIndex index);
		IReadOnlyBTreeIndex GetIndex(BarbadosIdentifier collection, BarbadosIdentifier field);

		void CreateCollection(BarbadosIdentifier name);
		void RemoveCollection(BarbadosIdentifier name);
		void RenameCollection(BarbadosIdentifier name, BarbadosIdentifier replacement);

		void CreateIndex(BarbadosIdentifier collection, BarbadosIdentifier field);
		void CreateIndex(BarbadosIdentifier collection, BarbadosIdentifier field, int maxKeyLength);
		void RemoveIndex(BarbadosIdentifier collection, BarbadosIdentifier field);
	}
}
