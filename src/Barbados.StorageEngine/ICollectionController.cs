using System.Collections.Generic;

using Barbados.StorageEngine.Collections;

namespace Barbados.StorageEngine
{
	public interface ICollectionController
	{
		void EnsureCreated(BarbadosIdentifier collectionName);
		void EnsureDeleted(BarbadosIdentifier collectionName);

		IEnumerable<string> List();

		bool Exists(ObjectId collectionId);
		bool Exists(BarbadosIdentifier collectionName);

		IBarbadosCollection Get(ObjectId collectionId);
		IBarbadosCollection Get(BarbadosIdentifier collectionName);

		void Rename(ObjectId collectionId, BarbadosIdentifier replacement);
		void Delete(ObjectId collectionId);
		void Create(BarbadosIdentifier collectionName);
		void Rename(BarbadosIdentifier collectionName, BarbadosIdentifier replacement);
		void Delete(BarbadosIdentifier collectionName);
	}
}
