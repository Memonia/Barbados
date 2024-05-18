using System;

using Barbados.StorageEngine.Caching;

namespace Barbados.StorageEngine
{
	public sealed class StorageOptionsBuilder
	{
		private int _cachedPageCountLimit;
		private CachingStrategy _cachingStrategy;

		public StorageOptionsBuilder()
		{
			_cachedPageCountLimit = 4096;
			_cachingStrategy = CachingStrategy.Default;
		}

		public StorageOptions Build()
		{
			return new StorageOptions
			{
				CachedPageCountLimit = _cachedPageCountLimit,
				CachingStrategy = _cachingStrategy
			};
		}

		public StorageOptionsBuilder WithCache(CachingStrategy cachingStrategy)
		{
			_cachingStrategy = cachingStrategy;
			return this;
		}

		public StorageOptionsBuilder WithCachedPageCountLimit(int count)
		{
			if (count <= 0)
			{
				throw new ArgumentException("Expected a positive integer", nameof(count));
			}

			_cachedPageCountLimit = count;
			return this;
		}
	}
}
