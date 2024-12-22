using System.Collections.Generic;

using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine
{
	public interface IIndexController
	{
		void EnsureCreated(ObjectId collectionId, string field);
		void EnsureCreated(ObjectId collectionId, string field, int maxKeyLength);
		void EnsureDeleted(ObjectId collectionId, string field);
		void EnsureCreated(BarbadosDbObjectName collectionName, string field);
		void EnsureCreated(BarbadosDbObjectName collectionName, string field, int maxKeyLength);
		void EnsureDeleted(BarbadosDbObjectName collectionName, string field);

		bool Exists(ObjectId collectionId, string field);
		bool Exists(BarbadosDbObjectName collectionName, string field);

		IEnumerable<string> ListIndexed(ObjectId collectionId);
		IEnumerable<string> ListIndexed(BarbadosDbObjectName collectionName);

		IReadOnlyBTreeIndex Get(ObjectId collectionId, string field);
		IReadOnlyBTreeIndex Get(BarbadosDbObjectName collectionName, string field);

		void Create(ObjectId collectionId, string field);
		void Create(ObjectId collectionId, string field, int maxKeyLength);
		void Delete(ObjectId collectionId, string field);
		void Create(BarbadosDbObjectName collectionName, string field);
		void Create(BarbadosDbObjectName collectionName, string field, int maxKeyLength);
		void Delete(BarbadosDbObjectName collectionName, string field);
	}
}
