using System.Collections.Generic;

using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine
{
	public interface IIndexController
	{
		void EnsureCreated(ObjectId collectionId, string field);
		void EnsureCreated(ObjectId collectionId, string field, int maxKeyLength);
		void EnsureDeleted(ObjectId collectionId, string field);
		void EnsureCreated(BarbadosIdentifier collectionName, string field);
		void EnsureCreated(BarbadosIdentifier collectionName, string field, int maxKeyLength);
		void EnsureDeleted(BarbadosIdentifier collectionName, string field);

		bool Exists(ObjectId collectionId, string field);
		bool Exists(BarbadosIdentifier collectionName, string field);

		IEnumerable<string> ListIndexed(ObjectId collectionId);
		IEnumerable<string> ListIndexed(BarbadosIdentifier collectionName);

		IReadOnlyBTreeIndex Get(ObjectId collectionId, string field);
		IReadOnlyBTreeIndex Get(BarbadosIdentifier collectionName, string field);

		void Create(ObjectId collectionId, string field);
		void Create(ObjectId collectionId, string field, int maxKeyLength);
		void Delete(ObjectId collectionId, string field);
		void Create(BarbadosIdentifier collectionName, string field);
		void Create(BarbadosIdentifier collectionName, string field, int maxKeyLength);
		void Delete(BarbadosIdentifier collectionName, string field);
	}
}
