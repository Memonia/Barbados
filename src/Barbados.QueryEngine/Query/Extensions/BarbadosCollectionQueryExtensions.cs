using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Query.Extensions
{
	public static class BarbadosCollectionQueryExtensions
	{
		public static Query Load(this IReadOnlyBarbadosCollection collection)
		{
			return new Query(collection, new QueryPlanBuilder(KeySelection.All));
		}

		public static Query LoadInclude(this IReadOnlyBarbadosCollection collection, params string[] keys)
		{
			return new Query(collection, new QueryPlanBuilder(new(keys, keysIncluded: true)));
		}

		public static Query LoadExclude(this IReadOnlyBarbadosCollection collection, params string[] keys)
		{
			return new Query(collection, new QueryPlanBuilder(new(keys, keysIncluded: false)));
		}
	}
}
