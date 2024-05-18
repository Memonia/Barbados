using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Indexing.Search.Checks
{
	internal sealed class KeyCheckEqual(NormalisedValue bound) : KeyCheckBound(bound)
	{
		protected override bool Evaluate(int result)
		{
			return result == 0;
		}
	}
}
