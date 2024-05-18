using System;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Indexing.Search.Checks;

namespace Barbados.StorageEngine.Indexing.Search
{
	internal static class KeyCheckFactory
	{
		public static IKeyCheck GetCheck(NormalisedValue bound, KeyCheckCondition condition)
		{
			return condition switch
			{
				KeyCheckCondition.Equal => new KeyCheckEqual(bound),
				KeyCheckCondition.LessThan => new KeyCheckLessThan(bound),
				KeyCheckCondition.GreaterThan => new KeyCheckGreaterThan(bound),
				KeyCheckCondition.LessThanOrEqual => new KeyCheckLessThanOrEqual(bound),
				KeyCheckCondition.GreaterThanOrEqual => new KeyCheckGreaterThanOrEqual(bound),
				_ => throw new NotImplementedException(),
			};
		}

		public static IKeyCheck GetBetweenCheck(NormalisedValue lower, NormalisedValue upper, KeyCheckCondition condition)
		{
			return condition switch
			{
				KeyCheckCondition.Between => new KeyCheckBetween(lower, upper),
				KeyCheckCondition.BetweenInclusive => new KeyCheckBetweenInclusive(lower, upper),
				_ => throw new NotImplementedException(),
			};
		}
	}
}
