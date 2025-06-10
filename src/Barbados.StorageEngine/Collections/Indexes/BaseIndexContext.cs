using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Collections.Indexes
{
	internal abstract partial class BaseIndexContext
	{
		protected IndexInfo Info { get; set; }
		protected BTreeContext Context { get; set; }

		public BaseIndexContext(IndexInfo info, BTreeContext context)
		{
			Info = info;
			Context = context;
		}

		public void Deallocate()
		{
			Context.Deallocate();
		}

		public abstract Enumerator GetEnumerator(BTreeFindOptions options);
		public abstract bool TryInsert(BTreeNormalisedValueSpan primaryKey, BTreeNormalisedValueSpan key);
		public abstract bool TryRemove(BTreeNormalisedValueSpan primaryKey, BTreeNormalisedValueSpan key);
	}
}
