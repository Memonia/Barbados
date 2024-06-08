using Barbados.StorageEngine.Caching;

namespace Barbados.StorageEngine
{
	public sealed record StorageOptions
	{
		public static StorageOptions Default { get; } = new StorageOptionsBuilder().Build();

		public required int CachedPageCountLimit { get; init; }
		public required CachingStrategy CachingStrategy { get; init; }
	}
}
