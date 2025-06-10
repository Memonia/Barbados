using System.Collections.Generic;

namespace Barbados.QueryEngine
{
	internal sealed class KeySelection
	{
		public static KeySelection All { get; } = new([], keysIncluded: true, selectAll: true);

		public bool SelectAll { get; }
		public bool KeysIncluded { get; }
		public IReadOnlyList<string> Keys { get; }

		public KeySelection(IReadOnlyList<string> keys, bool keysIncluded) : this(keys, keysIncluded, false)
		{

		}

		private KeySelection(IReadOnlyList<string> keys, bool keysIncluded, bool selectAll)
		{
			Keys = keys;
			KeysIncluded = keysIncluded;
			SelectAll = selectAll;
		}
	}
}
