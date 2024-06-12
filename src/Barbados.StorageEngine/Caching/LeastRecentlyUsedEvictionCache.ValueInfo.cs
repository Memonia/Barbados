namespace Barbados.StorageEngine.Caching
{
	internal partial class LeastRecentlyUsedEvictionCache<K, V>
	{
		private sealed record ValueInfo
		{
			public required K Key { get; init; }
			public required V Value { get; set; }

			public int Pins { get; set; }
			public bool Dirty { get; set; }
		}
	}
}
