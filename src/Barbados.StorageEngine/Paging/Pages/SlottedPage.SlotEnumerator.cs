namespace Barbados.StorageEngine.Paging.Pages
{
	internal partial class SlottedPage
	{
		protected ref struct SlotEnumerator(SlottedPage page)
		{
			private readonly SlottedPage _page = page;
			private int _currentIndex = -1;

			public bool TryGetNext(out Descriptor descriptor, out Slot slot)
			{
				if (_currentIndex < _page._descriptors.Count - 1)
				{
					_currentIndex += 1;

					descriptor = _page._descriptors[_currentIndex];
					slot = _page.GetSlot(descriptor);
					return true;
				}

				descriptor = default!;
				slot = default!;
				return false;
			}
		}
	}
}
