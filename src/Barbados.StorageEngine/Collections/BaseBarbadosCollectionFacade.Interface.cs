using Barbados.Documents;

namespace Barbados.StorageEngine.Collections
{
	internal partial class BaseBarbadosCollectionFacade
	{
		ICursor<BarbadosDocument> IReadOnlyBarbadosCollection.GetCursor()
		{
			return GetCursor();
		}

		ICursor<BarbadosDocument> IReadOnlyBarbadosCollection.GetCursor(BarbadosKeySelector selector)
		{
			return GetCursor(selector);
		}
	}
}
