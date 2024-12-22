using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Query.Extensions
{
	public static class IBarbadosCollectionQueryExtensions
	{
		public static IQuery Load(this IReadOnlyBarbadosCollection collection)
		{
			return Load(collection, BarbadosKeySelector.SelectAll);
		}

		public static IQuery Load(this IReadOnlyBarbadosCollection collection, params string[] selection)
		{
			return Load(collection, new BarbadosKeySelector(selection.Select(e => new BarbadosKey(e))));
		}

		public static IQuery Load(this IReadOnlyBarbadosCollection collection, BarbadosKeySelector selector)
		{
			return new Query(collection, new QueryPlanBuilder(selector));
		}
	}
}
