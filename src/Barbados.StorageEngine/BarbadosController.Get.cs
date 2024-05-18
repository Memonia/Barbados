using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine
{
	internal partial class BarbadosController
	{
		IBarbadosCollection IBarbadosController.GetCollection(BarbadosIdentifier name)
		{
			return GetCollection(name.Identifier);
		}

		bool IBarbadosController.TryGetCollection(BarbadosIdentifier name, out IBarbadosCollection collection)
		{
			if (TryGetCollection(name, out BarbadosCollection barbadosCollection))
			{
				collection = barbadosCollection;
				return true;
			}

			collection = default!;
			return false;
		}

		public IBarbadosReadOnlyCollection GetReadOnlyCollection(BarbadosIdentifier name)
		{
			if (name == BarbadosIdentifiers.Collection.MetaCollection)
			{
				return GetMetaCollection();
			}

			return GetCollection(name);
		}

		public bool TryGetReadOnlyCollection(BarbadosIdentifier name, out IBarbadosReadOnlyCollection collection)
		{
			if (name == BarbadosIdentifiers.Collection.MetaCollection)
			{
				collection = GetMetaCollection();
				return true;
			}

			if (TryGetCollection(name, out BarbadosCollection barbadosCollection))
			{
				collection = barbadosCollection;
				return true;
			}

			collection = default!;
			return false;
		}

		IBTreeIndexLookup IBarbadosController.GetIndex(BarbadosIdentifier collection, BarbadosIdentifier field)
		{
			return GetIndex(collection.Identifier, field.Identifier);
		}

		bool IBarbadosController.TryGetIndex(BarbadosIdentifier collection, BarbadosIdentifier field, out IBTreeIndexLookup index)
		{
			if (TryGetIndex(collection.Identifier, field.Identifier, out BTreeIndex bTreeIndex))
			{
				index = bTreeIndex;
				return true;
			}

			index = default!;
			return false;
		}
	}
}
