using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barbados.StorageEngine.Caching
{
	internal sealed partial class LeastRecentlyUsedEvictionCache<K, V> : ICache<K, V>
		where K : notnull
	{
		/* A simple cache with LRU eviction
		 */

		public event Action<K, V>? OnDirtyValueEviction;

		public int Count => _entries.Count;
		public ICollection<K> Keys => _entries.Keys;

		public int MaxCount { get; }

		// For _evictionList
		private readonly object _sync;

		// Most recently accessed value is stored at the head of the list
		private readonly LinkedList<ValueInfo> _evictionList;
		private readonly ConcurrentDictionary<K, LinkedListNode<ValueInfo>> _entries;

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
			if (_entries.TryGetValue(key, out var node))
			{
				lock (_sync)
				{
					_refresh(node);
					node.Value.Value = value;
				}

				return true;
			}

			if (Count < MaxCount || _tryEvict())
			{
				lock (_sync)
				{
					node = _evictionList.AddFirst(
						new ValueInfo() { Key = key, Value = value }
					);

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
				lock (_sync)
				{
					_refresh(node);
					value = node.Value.Value!;
					return true;
				}
			}

			value = default!;
			return false;
		}

		public bool TryGetWithPin(K key, out V value)
		{
			if (_entries.TryGetValue(key, out var node))
			{
				Pin(key);
				value = node.Value.Value!;
				return true;
			}

			value = default!;
			return false;
		}

		public bool TryPop(K key, out V value)
		{
			if (_entries.Remove(key, out var node))
			{
				lock (_sync)
				{
					_evictionList.Remove(node);
					value = node.Value.Value!;
					return true;
				}
			}

			value = default!;
			return false;
		}

		public void MarkDirty(K key)
		{
			if (_entries.TryGetValue(key, out var node))
			{
				lock (_sync)
				{
					_refresh(node);
					node.Value.Dirty = true;
				}
			}
		}

		public void MarkClean(K key)
		{
			if (_entries.TryGetValue(key, out var node))
			{
				lock (_sync)
				{
					_refresh(node);
					node.Value.Dirty = false;
				}
			}
		}

		public void Pin(K key)
		{
			if (_entries.TryGetValue(key, out var node))
			{
				lock (_sync)
				{
					node.Value.Pin = true;
					if (node.List is not null)
					{
						_evictionList.Remove(node);
					}
				}
			}
		}

		public void Release(K key)
		{
			if (_entries.TryGetValue(key, out var node))
			{
				lock (_sync)
				{
					node.Value.Pin = false;
					if (node.List is null)
					{
						_evictionList.AddFirst(node);
					}
				}
			}
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

					_evictionList.RemoveLast();
					_entries.Remove(node!.Value.Key, out _);

					if (node!.Value.Dirty)
					{
						OnDirtyValueEviction?.Invoke(node.Value.Key, node.Value.Value!);
					}

					return true;
				}

				return false;
			}
		}

		private void _refresh(LinkedListNode<ValueInfo> node)
		{
			if (!node.Value.Pin)
			{
				_evictionList.Remove(node);
				_evictionList.AddFirst(node);
			}
		}
	}
}
