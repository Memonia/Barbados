using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine.Indexing.Search
{
	internal interface IKeyCheck
	{
		bool Check(NormalisedValueSpan key);
	}
}
