using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal partial class BTreeLeafPage
	{
		public ref struct Enumerator(BTreeLeafPage page)
		{
			private readonly BTreeLeafPage _page = page;
			private KeyEnumerator _keyEnumerator = page.GetKeyEnumerator();

			public bool TryGetNext(out BTreeIndexKey indexKey)
			{
				if (_keyEnumerator.TryGetNext(out var key))
				{
					var r = _page.TryRead(key, out _, out var flags);
					Debug.Assert(r);

					var eflags = new Flags(flags);
					indexKey = new BTreeIndexKey(NormalisedValueSpan.FromNormalised(key), eflags.IsTrimmed);
					return true;
				}

				indexKey = default!;
				return false;
			}
		}
	}
}
