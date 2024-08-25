using Barbados.StorageEngine.Caching;

namespace Barbados.StorageEngine.Tests.Caching
{
	public sealed class LeastRecentlyUsedEvictionCacheTest
	{

		[Fact]
		public void CacheExactCount_ReadBackSuccess()
		{
			var cache = new LeastRecentlyUsedEvictionCache<int, string>(3);
			var (k1, v1) = (1, "str1");
			var (k2, v2) = (2, "str2");
			var (k3, v3) = (3, "str3");

			var tc1 = cache.TryCache(k1, v1);
			var tc2 = cache.TryCache(k2, v2);
			var tc3 = cache.TryCache(k3, v3);

			var tg1 = cache.TryGet(k1, out var g1);
			var tg2 = cache.TryGet(k2, out var g2);
			var tg3 = cache.TryGet(k3, out var g3);

			Assert.True(tc1);
			Assert.True(tc2);
			Assert.True(tc3);
			Assert.True(tg1);
			Assert.True(tg2);
			Assert.True(tg3);
			Assert.Equal(v1, g1);
			Assert.Equal(v2, g2);
			Assert.Equal(v3, g3);
		}

		[Fact]
		public void CacheOverCount_FirstCachedEvicted()
		{
			var cache = new LeastRecentlyUsedEvictionCache<int, string>(3);
			var (k1, v1) = (1, "str1");
			var (k2, v2) = (2, "str2");
			var (k3, v3) = (3, "str3");
			var (k4, v4) = (4, "str4");

			var tc1 = cache.TryCache(k1, v1);
			var tc2 = cache.TryCache(k2, v2);
			var tc3 = cache.TryCache(k3, v3);
			var tc4 = cache.TryCache(k4, v4);

			var tg1 = cache.TryGet(k1, out var g1);
			var tg2 = cache.TryGet(k2, out var g2);
			var tg3 = cache.TryGet(k3, out var g3);
			var tg4 = cache.TryGet(k4, out var g4);

			Assert.True(tc1);
			Assert.True(tc2);
			Assert.True(tc3);
			Assert.True(tc4);
			Assert.False(tg1);
			Assert.True(tg2);
			Assert.True(tg3);
			Assert.True(tg4);
			Assert.Equal(v2, g2);
			Assert.Equal(v3, g3);
			Assert.Equal(v4, g4);
		}

		[Fact]
		public void CacheOverCount_AccessFirstCached_EvictedSecondCached()
		{
			var cache = new LeastRecentlyUsedEvictionCache<int, string>(3);
			var (k1, v1) = (1, "str1");
			var (k2, v2) = (2, "str2");
			var (k3, v3) = (3, "str3");
			var (k4, v4) = (4, "str4");

			var tc1 = cache.TryCache(k1, v1);
			var tc2 = cache.TryCache(k2, v2);
			var tc3 = cache.TryCache(k3, v3);

			var tg1_1 = cache.TryGet(k1, out var g1_1);

			var tc4 = cache.TryCache(k4, v4);

			var tg1_2 = cache.TryGet(k1, out var g1_2);
			var tg2 = cache.TryGet(k2, out var g2);
			var tg3 = cache.TryGet(k3, out var g3);
			var tg4 = cache.TryGet(k4, out var g4);

			Assert.True(tc1);
			Assert.True(tc2);
			Assert.True(tc3);
			Assert.True(tc4);
			Assert.True(tg1_1);
			Assert.True(tg1_2);
			Assert.False(tg2);
			Assert.True(tg3);
			Assert.True(tg4);
			Assert.Equal(v1, g1_1);
			Assert.Equal(v1, g1_2);
			Assert.Equal(v3, g3);
			Assert.Equal(v4, g4);
		}
	}
}
