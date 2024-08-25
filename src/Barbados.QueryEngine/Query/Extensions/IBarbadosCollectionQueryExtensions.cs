using Barbados.StorageEngine;
using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Query.Extensions
{
	public static class IBarbadosCollectionQueryExtensions
	{
		public static IQuery Load(this IReadOnlyBarbadosCollection collection)
		{
			return Load(collection, ValueSelector.SelectAll);
		}

		public static IQuery Load(this IReadOnlyBarbadosCollection collection, params BarbadosIdentifier[] selection)
		{
			return Load(collection, new ValueSelector(selection));
		}

		public static IQuery Load(this IReadOnlyBarbadosCollection collection, ValueSelector selector)
		{
			return new Query(collection, new QueryPlanBuilder(selector));
		}
	}
}
