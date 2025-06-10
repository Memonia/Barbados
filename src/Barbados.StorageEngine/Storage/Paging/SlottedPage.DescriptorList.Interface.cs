using System;

namespace Barbados.StorageEngine.Storage.Paging
{
	internal partial class SlottedPage
	{
		protected partial struct DescriptorList
		{
			int IActiveDescriptorList.Count => ActiveCount;
			int IActiveDescriptorList.BinarySearch(ReadOnlySpan<byte> key) => ActiveDescriptorBinarySearch(key);

			bool IActiveDescriptorList.TryGetLowest(out Descriptor descriptor)
			{
				if (_activeDescriptors.Count == 0)
				{
					descriptor = default;
					return false;
				}

				descriptor = _activeDescriptors[0];
				return true;
			}

			bool IActiveDescriptorList.TryGetHighest(out Descriptor descriptor)
			{
				if (_activeDescriptors.Count == 0)
				{
					descriptor = default;
					return false;
				}

				descriptor = _activeDescriptors[^1];
				return true;
			}

			Descriptor IActiveDescriptorList.Get(int index) => GetActive(index);
		}
	}
}
