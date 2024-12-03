namespace Barbados.StorageEngine.Indexing.Search.Checks
{
	internal sealed class KeyCheckBetween(NormalisedValue lowerBound, NormalisedValue upperBound) : KeyCheckRange(lowerBound, upperBound)
	{
		protected override bool Evaluate(int lower, int upper)
		{
			return lower > 0 && upper < 0;
		}
	}
}
