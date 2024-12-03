using System;

namespace Barbados.StorageEngine.Indexing.Search
{
	internal abstract class KeyCheckRange(NormalisedValue lowerBound, NormalisedValue upperBound) : IKeyCheck
	{
		public NormalisedValue LowerBound { get; } = lowerBound;
		public NormalisedValue UpperBound { get; } = upperBound;

		public bool Check(NormalisedValueSpan key)
		{
			return Evaluate(
				key.Bytes.SequenceCompareTo(LowerBound.AsSpan().Bytes),
				key.Bytes.SequenceCompareTo(UpperBound.AsSpan().Bytes)
			);
		}

		protected abstract bool Evaluate(int lower, int upper);
	}
}
