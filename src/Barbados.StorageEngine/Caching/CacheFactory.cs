using System;

namespace Barbados.StorageEngine.Caching
{
	internal sealed class CacheFactory(int maxCount, CachingStrategy strategy)
	{
		private readonly int _maxCount = maxCount;
		private readonly CachingStrategy _strategy = strategy;

		public ICache<K, V> GetCache<K, V>()
			where K : notnull
			where V : class
		{
			return _strategy switch
			{
				CachingStrategy.LeastRecentlyUsedEviction => new LeastRecentlyUsedEvictionCache<K, V>(_maxCount),
				_ => throw new NotImplementedException()
			};
		}
	}
}
