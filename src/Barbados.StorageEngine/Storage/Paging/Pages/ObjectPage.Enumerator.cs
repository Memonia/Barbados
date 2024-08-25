namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal partial class ObjectPage
	{
		public ref struct Enumerator(ObjectPage page)
		{
			private KeyEnumerator _keyEnumerator = page.GetKeyEnumerator();

			public bool TryGetNext(out ObjectId id)
			{
				if (_keyEnumerator.TryGetNext(out var key))
				{
					id = ObjectIdNormalised.FromNormalised(key);
					return true;
				}

				id = default!;
				return false;
			}
		}
	}
}
