using System.Collections.Generic;

namespace Barbados.StorageEngine.Caching
{
	internal interface ICache<K, V> 
		where K : notnull 
		where V : class
	{
		int Count { get; }
		int MaxCount { get; }

		ICollection<K> Keys { get; }

		bool ContainsKey(K key);
		bool TryCache(K key, V value);
		bool TryGet(K key, out V value);
	}
}
