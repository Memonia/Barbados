using System;

namespace Barbados.StorageEngine.Storage.Paging
{
	internal partial class SlottedPage
	{
		protected interface IActiveDescriptorList
		{
			int Count { get; }
			int BinarySearch(ReadOnlySpan<byte> key);

			bool TryGetLowest(out Descriptor descriptor);
			bool TryGetHighest(out Descriptor descriptor);

			Descriptor Get(int index);
		}
	}
}
