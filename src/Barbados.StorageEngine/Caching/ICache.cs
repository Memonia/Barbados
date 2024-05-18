using System;
using System.Collections.Generic;

namespace Barbados.StorageEngine.Caching
{
	public interface ICache<K, V>
	{
		event Action<K, V>? OnDirtyValueEviction;

		int Count { get; }
		int MaxCount { get; }

		ICollection<K> Keys { get; }

		bool TryCache(K key, V value);

		bool TryGet(K key, out V value);

		bool TryGetWithPin(K key, out V value);

		bool TryPop(K key, out V value);

		void MarkDirty(K key);

		void MarkClean(K key);

		void Pin(K key);

		void Release(K key);

		bool ContainsKey(K key);
	}
}
