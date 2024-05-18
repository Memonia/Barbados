using System;

namespace Barbados.StorageEngine.Caching
{
	public sealed class CacheFactory(int maxCount, CachingStrategy strategy)
	{
		private readonly int _maxCount = maxCount;
		private readonly CachingStrategy _strategy = strategy;

		public ICache<TKey, TValue> GetCache<TKey, TValue>()
			where TKey : notnull
		{
			return _strategy switch
			{
				CachingStrategy.LeastRecentlyUsedEviction => new LeastRecentlyUsedEvictionCache<TKey, TValue>(_maxCount),
				_ => throw new NotImplementedException()
			};
		}
	}
}
