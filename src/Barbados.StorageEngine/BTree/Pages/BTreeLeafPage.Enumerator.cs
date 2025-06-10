namespace Barbados.StorageEngine.BTree.Pages
{
	internal partial class BTreeLeafPage
	{
		public ref struct Enumerator
		{
			// !!! no readonly: defensive copies
			private KeyEnumerator _keyEnumerator;

			public Enumerator(BTreeLeafPage page)
			{
				_keyEnumerator = page.GetKeyEnumerator();
			}

			public bool TryGetNext(out BTreeLookupKeySpan key)
			{
				if (_keyEnumerator.TryGetNext(out var ekey, out var flags))
				{
					key = new(BTreeNormalisedValueSpan.FromNormalised(ekey), ((Flags)flags).IsKeyTrimmed);
					return true;
				}

				key = default!;
				return false;
			}
		}
	}
}
