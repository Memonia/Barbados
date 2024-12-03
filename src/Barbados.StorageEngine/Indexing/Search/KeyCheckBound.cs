using System;

namespace Barbados.StorageEngine.Indexing.Search
{
	internal abstract class KeyCheckBound(NormalisedValue bound) : IKeyCheck
	{
		public NormalisedValue Bound { get; } = bound;

		public bool Check(NormalisedValueSpan key)
		{
			return Evaluate(
				key.Bytes.SequenceCompareTo(Bound.AsSpan().Bytes)
			);
		}

		protected abstract bool Evaluate(int result);
	}
}
