using System.Collections.Concurrent;
using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Exceptions;

using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine
{
	internal sealed class IndexControllerService
	{
		private readonly ConcurrentDictionary<ObjectId, ConcurrentDictionary<BarbadosKey, BTreeIndexFacade>> _facades;

		public IndexControllerService()
		{
			_facades = [];
		}

		public void AddFacade(BTreeIndexFacade facade)
		{
			var facades = _facades.GetOrAdd(facade.CollectionId, _ => []);
			if (!facades.TryAdd(facade.IndexField, facade))
			{
				throw new BarbadosInternalErrorException();
			}
		}

		public bool TryGetFacade(ObjectId collectionId, string field, out BTreeIndexFacade facade)
		{
			var facades = _facades.GetOrAdd(collectionId, _ => []);
			if (facades.TryGetValue(field, out facade!))
			{
				return true;
			}

			facade = default!;
			return false;
		}

		public bool TryRemoveFacade(ObjectId collectionId, string field, out BTreeIndexFacade facade)
		{
			var facades = _facades.GetOrAdd(collectionId, _ => []);
			if (facades.TryRemove(field, out facade!))
			{
				facade.IsDeleted = true;
				return true;
			}

			facade = default!;
			return false;
		}

		public bool TryRemoveFacades(ObjectId collectionId)
		{
			if (!_facades.TryRemove(collectionId, out var facades))
			{
				return false;
			}

			foreach (var facade in facades.Values)
			{
				facade.IsDeleted = true;
			}

			return true;
		}

		public IEnumerable<BTreeIndexFacade> EnumerateBTreeIndexFacades(BTreeClusteredIndexFacade clusteredIndexFacade)
		{
			if (!_facades.TryGetValue(clusteredIndexFacade.Info.CollectionId, out var facades))
			{
				yield break;
			}

			foreach (var facade in facades.Values)
			{
				yield return facade;
			}
		}
	}
}
