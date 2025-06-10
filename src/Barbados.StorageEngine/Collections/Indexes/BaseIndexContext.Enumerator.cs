using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Collections.Indexes
{
	internal partial class BaseIndexContext
	{
		public abstract class Enumerator
		{
			abstract public bool MoveNext();
			abstract public bool TryGetCurrent(out BTreeNormalisedValue key);
			abstract public bool TryGetCurrentAsSpan(out BTreeNormalisedValueSpan key);
		}
	}
}
