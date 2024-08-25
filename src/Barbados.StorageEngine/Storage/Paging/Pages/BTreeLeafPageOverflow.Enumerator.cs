namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal partial class BTreeLeafPageOverflow
	{
		public ref struct Enumerator(BTreeLeafPageOverflow page)
		{
			private SlotEnumerator _dataEnumerator = page.GetSlotEnumerator();

			public bool TryGetNext(out ObjectId id, out bool isTrimmed)
			{
				while (_dataEnumerator.TryGetNext(out var descriptor, out var slot))
				{
					if (!descriptor.IsGarbage)
					{
						var eflags = new BTreeLeafPage.Flags(descriptor.CustomFlags);
						id = ObjectIdNormalised.FromNormalised(slot.Key);
						isTrimmed = eflags.IsTrimmed;
						return true;
					}
				}

				id = default!;
				isTrimmed = default!;
				return false;
			}
		}
	}
}
