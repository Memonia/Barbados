namespace Barbados.StorageEngine.BTree.Pages
{
	internal partial class BTreePage
	{
		public ref struct Enumerator
		{
			// !!! no readonly: defensive copies
			private KeyEnumerator _keyEnumerator;

			public Enumerator(BTreePage page)
			{
				_keyEnumerator = page.GetKeyEnumerator();
			}

			public bool TryGetNext(out BTreeNormalisedValueSpan separator)
			{
				if (_keyEnumerator.TryGetNext(out var key, out _))
				{
					separator = BTreeNormalisedValueSpan.FromNormalised(key);
					return true;
				}

				separator = default!;
				return false;
			}
		}
	}
}
