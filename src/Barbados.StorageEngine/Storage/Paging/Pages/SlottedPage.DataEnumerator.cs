using System;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal partial class SlottedPage
	{
		protected ref struct DataEnumerator(SlotEnumerator slotEnumerator)
		{
			private SlotEnumerator _slotEnumerator = slotEnumerator;

			public bool TryGetNext(out Span<byte> data, out byte flags)
			{
				while (_slotEnumerator.TryGetNext(out var descriptor, out var slot))
				{
					if (!descriptor.IsGarbage)
					{
						data = slot.Data;
						flags = descriptor.CustomFlags;
						return true;
					}
				}

				data = default!;
				flags = default!;
				return false;
			}
		}
	}
}