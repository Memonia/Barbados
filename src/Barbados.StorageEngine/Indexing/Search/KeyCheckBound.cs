using System;

using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Indexing.Search
{
	internal abstract class KeyCheckBound(NormalisedValue bound) : IKeyCheck
	{
		private readonly NormalisedValue _bound = bound;

		public bool Check(NormalisedValueSpan key)
		{
			return Evaluate(
				key.Bytes.SequenceCompareTo(_bound.AsSpan().Bytes)
			);
		}

		protected abstract bool Evaluate(int result);
	}
}
