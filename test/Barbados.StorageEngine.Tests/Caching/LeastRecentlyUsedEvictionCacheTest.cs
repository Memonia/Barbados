using Barbados.StorageEngine.Caching;

namespace Barbados.StorageEngine.Tests.Caching
{
	public sealed class LeastRecentlyUsedEvictionCacheTest
	{
		[Test]
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

			Assert.Multiple(() =>
			{
				Assert.That(tc1, Is.True);
				Assert.That(tc2, Is.True);
				Assert.That(tc3, Is.True);
				Assert.That(tg1, Is.True);
				Assert.That(tg2, Is.True);
				Assert.That(tg3, Is.True);
				Assert.That(g1, Is.EqualTo(v1));
				Assert.That(g2, Is.EqualTo(v2));
				Assert.That(g3, Is.EqualTo(v3));
			});
		}

		[Test]
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

			Assert.Multiple(() =>
			{
				Assert.That(tc1, Is.True);
				Assert.That(tc2, Is.True);
				Assert.That(tc3, Is.True);
				Assert.That(tc4, Is.True);
				Assert.That(tg1, Is.False);
				Assert.That(tg2, Is.True);
				Assert.That(tg3, Is.True);
				Assert.That(tg4, Is.True);
				Assert.That(g2, Is.EqualTo(v2));
				Assert.That(g3, Is.EqualTo(v3));
				Assert.That(g4, Is.EqualTo(v4));
			});
		}

		[Test]
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

			Assert.Multiple(() =>
			{
				Assert.That(tc1, Is.True);
				Assert.That(tc2, Is.True);
				Assert.That(tc3, Is.True);
				Assert.That(tc4, Is.True);
				Assert.That(tg1_1, Is.True);
				Assert.That(tg1_2, Is.True);
				Assert.That(tg2, Is.False);
				Assert.That(tg3, Is.True);
				Assert.That(tg4, Is.True);
				Assert.That(g1_1, Is.EqualTo(v1));
				Assert.That(g1_2, Is.EqualTo(v1));
				Assert.That(g3, Is.EqualTo(v3));
				Assert.That(g4, Is.EqualTo(v4));
			});
		}
	}
}
