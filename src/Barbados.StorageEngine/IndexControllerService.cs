using System.Collections.Concurrent;
using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal sealed class IndexControllerService
	{
		private readonly ConcurrentDictionary<ObjectId, ConcurrentDictionary<BarbadosKey, IndexInfo>> _indexes;

		public IndexControllerService()
		{
			_indexes = [];
		}

		public void Add(ObjectId collectionId, IndexInfo info)
		{
			var indexes = _indexes.GetOrAdd(collectionId, _ => []);
			if (!indexes.TryAdd(info.Field, info))
			{
				throw new BarbadosInternalErrorException();
			}
		}

		public bool TryGet(ObjectId collectionId, BarbadosKey field, out IndexInfo info)
		{
			var indexes = _indexes.GetOrAdd(collectionId, _ => []);
			return indexes.TryGetValue(field, out info!);
		}

		public bool TryRemove(ObjectId collectionId)
		{
			return _indexes.TryRemove(collectionId, out _);
		}

		public bool TryRemove(ObjectId collectionId, BarbadosKey field, out IndexInfo info)
		{
			var indexes = _indexes.GetOrAdd(collectionId, _ => []);
			return indexes.TryRemove(field, out info!);
		}

		public IEnumerable<IndexInfo> EnumerateIndexes(ObjectId collectionId)
		{
			if (!_indexes.TryGetValue(collectionId, out var indexes))
			{
				yield break;
			}

			foreach (var index in indexes.Values)
			{
				yield return index;
			}
		}
	}
}
