using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal partial class BTreePage
	{
		public ref struct Enumerator(BTreePage page)
		{
			private KeyEnumerator _keyEnumerator = page.GetKeyEnumerator();

			public bool TryGetNext(out NormalisedValueSpan separator)
			{
				if (_keyEnumerator.TryGetNext(out var key))
				{
					separator = NormalisedValueSpan.FromNormalised(key);
					return true;
				}

				separator = default!;
				return false;
			}
		}
	}
}
