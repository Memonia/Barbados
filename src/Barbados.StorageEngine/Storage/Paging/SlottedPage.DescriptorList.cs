using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barbados.StorageEngine.Storage.Paging
{
	internal partial class SlottedPage
	{
		protected readonly partial struct DescriptorList : IActiveDescriptorList
		{
			public int Count => _descriptors.Count;
			public int ActiveCount => _activeDescriptors.Count;

			private readonly Func<Descriptor, Slot> _descriptorSlotMapper;
			private readonly List<Descriptor> _descriptors;
			private readonly List<Descriptor> _activeDescriptors;

			public DescriptorList(Func<Descriptor, Slot> descriptorSlotMapper)
			{
				_descriptorSlotMapper = descriptorSlotMapper;
				_descriptors = [];
				_activeDescriptors = [];
			}

			public void Clear()
			{
				_descriptors.Clear();
				_activeDescriptors.Clear();
			}

			public int ActiveDescriptorBinarySearch(ReadOnlySpan<byte> key)
			{
				return _descriptorBinarySearch(_activeDescriptors, key);
			}

			public bool TryGetActive(ReadOnlySpan<byte> key, out Descriptor descriptor)
			{
				var i = _descriptorBinarySearch(_activeDescriptors, key);
				if (i < 0)
				{
					descriptor = default!;
					return false;
				}

				descriptor = _activeDescriptors[i];
				return true;
			}

			public bool TryGetActive(ReadOnlySpan<byte> key, out int absoluteIndex, out Descriptor descriptor)
			{
				absoluteIndex = _descriptorBinarySearch(_descriptors, key);
				if (absoluteIndex < 0)
				{
					descriptor = default;
					return false;
				}

				Debug.Assert(absoluteIndex >= 0);
				if (!_descriptors[absoluteIndex].IsGarbage)
				{
					descriptor = _descriptors[absoluteIndex];
					return true;
				}

				// Check the left side of the found descriptor
				for (int i = absoluteIndex - 1; i >= 0; --i)
				{
					descriptor = _descriptors[i];
					if (!_descriptorSlotMapper(descriptor).Key.SequenceEqual(key))
					{
						break;
					}

					if (!descriptor.IsGarbage)
					{
						absoluteIndex = i;
						return true;
					}
				}

				// Check the right side of the found descriptor
				for (int i = absoluteIndex + 1; i < _descriptors.Count; ++i)
				{
					descriptor = _descriptors[i];
					if (!_descriptorSlotMapper(descriptor).Key.SequenceEqual(key))
					{
						break;
					}

					if (!descriptor.IsGarbage)
					{
						absoluteIndex = i;
						return true;
					}
				}

				descriptor = default!;
				return false;
			}

			public Descriptor Get(int index)
			{
				return _descriptors[index];
			}

			public Descriptor GetActive(int index)
			{
				return _activeDescriptors[index];
			}

			public void Add(Descriptor descriptor)
			{
				var key = _descriptorSlotMapper(descriptor).Key;
				Add(key, descriptor);
			}

			public void Add(ReadOnlySpan<byte> key, Descriptor descriptor)
			{
				_add(_descriptors, key, descriptor);
				if (!descriptor.IsGarbage)
				{
					_add(_activeDescriptors, key, descriptor);
				}
			}

			public void Update(int index, Descriptor descriptor)
			{
				var key = _descriptorSlotMapper(descriptor).Key;
				Update(index, key, descriptor);
			}

			public void Update(int index, ReadOnlySpan<byte> key, Descriptor descriptor)
			{
				var activeDescriptorIndex = _descriptorBinarySearch(_activeDescriptors, key);
				if (activeDescriptorIndex >= 0)
				{
					_activeDescriptors.RemoveAt(activeDescriptorIndex);
				}

				_descriptors.RemoveAt(index);
				Add(key, descriptor);
			}

			private int _descriptorBinarySearch(List<Descriptor> descriptors, ReadOnlySpan<byte> key)
			{
				int left = 0;
				int right = descriptors.Count - 1;
				while (left <= right)
				{
					// Idea taken from .NET runtime implementation
					int m = (int)((uint)left + (uint)right >> 1);

					var slot = _descriptorSlotMapper(descriptors[m]);
					int c = key.SequenceCompareTo(slot.Key);

					if (c == 0)
					{
						return m;
					}

					else
					if (c > 0)
					{
						left = m + 1;
					}

					else
					{
						right = m - 1;
					}
				}

				return ~left;
			}

			private void _add(List<Descriptor> descriptors, ReadOnlySpan<byte> key, Descriptor descriptor)
			{
				var index = _descriptorBinarySearch(descriptors, key);
				if (index < 0)
				{
					index = ~index;
				}

				descriptors.Insert(index, descriptor);
			}
		}
	}
}
