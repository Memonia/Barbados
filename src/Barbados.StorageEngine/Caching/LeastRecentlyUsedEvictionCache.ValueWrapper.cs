using System;

namespace Barbados.StorageEngine.Caching
{
	internal partial class LeastRecentlyUsedEvictionCache<K, V>
	{
		private sealed class ValueWrapper
		{
			public K Key { get; }
			public WeakReference<V> WeakRef { get; }

			public ValueWrapper(K key, V value)
			{
				Key = key;
				WeakRef = new(value);
			}
		}
	}
}
