using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Query.Extensions
{
	public static class IBarbadosCollectionQueryExtensions
	{
		public static IQuery Load(this IBarbadosCollection collection)
		{
			return Load(collection, ValueSelector.SelectAll);
		}

		public static IQuery Load(this IBarbadosCollection collection, params BarbadosIdentifier[] selection)
		{
			return Load(collection, new ValueSelector(selection));
		}

		public static IQuery Load(this IBarbadosCollection collection, ValueSelector selector)
		{
			return new Query(collection, new QueryPlanBuilder(selector));
		}
	}
}
