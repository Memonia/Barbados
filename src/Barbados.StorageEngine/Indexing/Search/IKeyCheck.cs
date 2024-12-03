namespace Barbados.StorageEngine.Indexing.Search
{
	internal interface IKeyCheck
	{
		bool Check(NormalisedValueSpan key);
	}
}
