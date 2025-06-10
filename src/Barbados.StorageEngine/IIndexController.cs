using System.Collections.Generic;

namespace Barbados.StorageEngine
{
	public interface IIndexController
	{
		void EnsureCreated(ObjectId collectionId, string field);
		void EnsureDeleted(ObjectId collectionId, string field);
		void EnsureCreated(BarbadosDbObjectName collectionName, string field);
		void EnsureDeleted(BarbadosDbObjectName collectionName, string field);

		bool Exists(ObjectId collectionId, string field);
		bool Exists(BarbadosDbObjectName collectionName, string field);

		IEnumerable<string> ListIndexed(ObjectId collectionId);
		IEnumerable<string> ListIndexed(BarbadosDbObjectName collectionName);

		void Create(ObjectId collectionId, string field);
		void Delete(ObjectId collectionId, string field);
		void Create(BarbadosDbObjectName collectionName, string field);
		void Delete(BarbadosDbObjectName collectionName, string field);
	}
}
