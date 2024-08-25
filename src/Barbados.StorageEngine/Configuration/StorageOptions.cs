using System;

using Barbados.StorageEngine.Caching;

namespace Barbados.StorageEngine.Configuration
{
	public sealed class StorageOptions
	{
		public static StorageOptions Default { get; } = new StorageOptionsBuilder().Build();

		public required CachingStrategy CachingStrategy { get; init; }
		public required int CachedPageCountLimit { get; init; }
		public required int WalPageCountLimit { get; init; }
		public required int WalBufferedPageCountLimit { get; init; }
		public required TimeSpan TransactionAcquireLockTimeout { get; init; }
	}
}
