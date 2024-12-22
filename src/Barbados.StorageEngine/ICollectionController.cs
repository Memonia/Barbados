using System.Collections.Generic;

using Barbados.StorageEngine.Collections;

namespace Barbados.StorageEngine
{
	public interface ICollectionController
	{
		void EnsureCreated(BarbadosDbObjectName collectionName);
		void EnsureDeleted(BarbadosDbObjectName collectionName);

		IEnumerable<string> List();

		bool Exists(ObjectId collectionId);
		bool Exists(BarbadosDbObjectName collectionName);

		IBarbadosCollection Get(ObjectId collectionId);
		IBarbadosCollection Get(BarbadosDbObjectName collectionName);

		void Rename(ObjectId collectionId, BarbadosDbObjectName replacement);
		void Delete(ObjectId collectionId);
		void Create(BarbadosDbObjectName collectionName);
		void Rename(BarbadosDbObjectName collectionName, BarbadosDbObjectName replacement);
		void Delete(BarbadosDbObjectName collectionName);
	}
}
