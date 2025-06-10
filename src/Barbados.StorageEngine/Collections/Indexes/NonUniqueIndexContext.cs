using System;

using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Storage;

namespace Barbados.StorageEngine.Collections.Indexes
{
	internal sealed partial class NonUniqueIndexContext : BaseIndexContext
	{
		public NonUniqueIndexContext(IndexInfo info, BTreeContext context) : base(info, context)
		{

		}

		public override BaseIndexContext.Enumerator GetEnumerator(BTreeFindOptions options)
		{
			return new Enumerator(this, options);
		}

		public override bool TryInsert(BTreeNormalisedValueSpan primaryKey, BTreeNormalisedValueSpan key)
		{
			var kc = new KeyConcat(primaryKey, key);
			Span<byte> indexKeyLengthBytes = stackalloc byte[sizeof(int)];
			HelpWrite.AsInt32(indexKeyLengthBytes, key.Bytes.Length);
			return Context.TryInsert(BTreeNormalisedValueSpan.FromNormalised(kc.Bytes), indexKeyLengthBytes);
		}

		public override bool TryRemove(BTreeNormalisedValueSpan primaryKey, BTreeNormalisedValueSpan key)
		{
			var kc = new KeyConcat(primaryKey, key);
			return Context.TryRemove(BTreeNormalisedValueSpan.FromNormalised(kc.Bytes));
		}
	}
}
