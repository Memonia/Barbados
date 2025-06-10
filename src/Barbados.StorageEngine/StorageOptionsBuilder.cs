using System;

using Barbados.StorageEngine.Caching;

namespace Barbados.StorageEngine
{
	public sealed class StorageOptionsBuilder
	{
		private CachingStrategy _cachingStrategy;
		private int _cachedPageCountLimit;
		private int _walPageCountLimit;
		private int _walBufferedPageCountLimit;
		private TimeSpan _transactionAcquireLockTimeout;

		public StorageOptionsBuilder()
		{
			_reset();
		}

		public StorageOptions Build()
		{
			var so = new StorageOptions
			{
				CachingStrategy = _cachingStrategy,
				CachedPageCountLimit = _cachedPageCountLimit,
				WalPageCountLimit = _walPageCountLimit,
				WalBufferedPageCountLimit = _walBufferedPageCountLimit,
				TransactionAcquireLockTimeout = _transactionAcquireLockTimeout
			};

			_reset();
			return so;
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

		public StorageOptionsBuilder WithWalPageCountLimit(int count)
		{
			if (count <= 0)
			{
				throw new ArgumentException("Expected a positive integer", nameof(count));
			}

			_walPageCountLimit = count;
			return this;
		}

		public StorageOptionsBuilder WithWalBufferedPageCountLimit(int count)
		{
			if (count <= 0)
			{
				throw new ArgumentException("Expected a positive integer", nameof(count));
			}

			_walBufferedPageCountLimit = count;
			return this;
		}

		private void _reset()
		{
			_cachingStrategy = CachingStrategy.Default;
			_cachedPageCountLimit = 4096;
			_walPageCountLimit = 1024;
			_walBufferedPageCountLimit = 256;
			_transactionAcquireLockTimeout = TimeSpan.FromMinutes(1);
		}
	}
}
