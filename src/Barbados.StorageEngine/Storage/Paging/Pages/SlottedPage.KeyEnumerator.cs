using System;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal partial class SlottedPage
	{
		protected ref struct KeyEnumerator
		{
			/* Only returns unique keys 
			 */

			private SlotEnumerator _slotEnumerator;
			private Slot _previous;
			private bool _stop;

			public KeyEnumerator(SlotEnumerator slotEnumerator)
			{
				_stop = true;
				_slotEnumerator = slotEnumerator;
				while (_slotEnumerator.TryGetNext(out var descriptor, out var slot))
				{
					if (!descriptor.IsGarbage)
					{
						_stop = false;
						_previous = slot;
						break;
					}
				}
			}

			public bool TryGetNext(out ReadOnlySpan<byte> key)
			{
				if (_stop)
				{
					key = default!;
					return false;
				}

				key = _previous.Key;
				while (_slotEnumerator.TryGetNext(out var descriptor, out var slot))
				{
					if (!descriptor.IsGarbage && !slot.Key.SequenceEqual(_previous.Key))
					{
						_previous = slot;
						return true;
					}
				}

				_stop = true;
				return true;
			}
		}
	}
}
