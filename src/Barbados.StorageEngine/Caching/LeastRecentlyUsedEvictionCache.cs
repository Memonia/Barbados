using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barbados.StorageEngine.Caching
{
	internal sealed partial class LeastRecentlyUsedEvictionCache<K, V> : ICache<K, V>
		where K : notnull
		where V : class
	{
		/* A simple cache with LRU eviction
		 */

		public int Count => _entries.Count;
		public ICollection<K> Keys => _entries.Keys;

		public int MaxCount { get; }

		// For _evictionList
		private readonly object _sync;

		// Most recently accessed value is stored at the head of the list
		private readonly LinkedList<ValueWrapper> _evictionList;
		private readonly ConcurrentDictionary<K, LinkedListNode<ValueWrapper>> _entries;

		public LeastRecentlyUsedEvictionCache(int maxCount)
		{
			if (maxCount <= 0)
			{
				throw new ArgumentException("Expected a positive integer", nameof(maxCount));
			}

			_sync = new();
			_entries = [];
			_evictionList = [];

			MaxCount = maxCount;
		}

		public bool TryCache(K key, V value)
		{
			if (_entries.TryRemove(key, out var node))
			{
				lock (_sync)
				{
					_refresh(node);
					node.Value = new(key, value);
				}

				var r = _entries.TryAdd(key, node);
				Debug.Assert(r);
				return true;
			}

			if (Count < MaxCount || _tryEvict())
			{
				lock (_sync)
				{
					node = _evictionList.AddFirst(new ValueWrapper(key, value));
					var r = _entries.TryAdd(key, node);
					Debug.Assert(r);
				}

				return true;
			}

			return false;
		}

		public bool TryGet(K key, out V value)
		{
			if (_entries.TryGetValue(key, out var node))
			{
				if (node.Value.WeakRef.TryGetTarget(out value!))
				{
					lock (_sync)
					{
						_refresh(node);
					}

					return true;
				}
			}

			value = default!;
			return false;
		}

		public bool ContainsKey(K key)
		{
			return _entries.ContainsKey(key);
		}

		private bool _tryEvict()
		{
			lock (_sync)
			{
				if (_evictionList.Count > 0)
				{
					var node = _evictionList.Last;
					Debug.Assert(node is not null);
					var r = _entries.TryRemove(node.Value.Key, out _);
					Debug.Assert(r);

					_evictionList.RemoveLast();
					return true;
				}

				return false;
			}
		}

		private void _refresh(LinkedListNode<ValueWrapper> node)
		{
			_evictionList.Remove(node);
			_evictionList.AddFirst(node);
		}
	}
}
