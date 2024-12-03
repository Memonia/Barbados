namespace Barbados.StorageEngine.Indexing.Search.Checks
{
	internal sealed class KeyCheckLessThan(NormalisedValue bound) : KeyCheckBound(bound)
	{
		protected override bool Evaluate(int result)
		{
			return result < 0;
		}
	}
}
