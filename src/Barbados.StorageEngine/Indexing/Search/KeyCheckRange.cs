using System;

using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Indexing.Search
{
	internal abstract class KeyCheckRange(NormalisedValue lowerBound, NormalisedValue upperBound) : IKeyCheck
	{
		private readonly NormalisedValue _lowerBound = lowerBound;
		private readonly NormalisedValue _upperBound = upperBound;

		public bool Check(NormalisedValueSpan key)
		{
			return Evaluate(
				key.Bytes.SequenceCompareTo(_lowerBound.AsSpan().Bytes),
				key.Bytes.SequenceCompareTo(_upperBound.AsSpan().Bytes)
			);
		}

		protected abstract bool Evaluate(int lower, int upper);
	}
}
