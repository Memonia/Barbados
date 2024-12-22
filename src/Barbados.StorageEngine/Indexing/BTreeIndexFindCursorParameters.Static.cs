using System;

using Barbados.Documents;
using Barbados.StorageEngine.Indexing.Extensions;
using Barbados.StorageEngine.Indexing.Search;
using Barbados.StorageEngine.Indexing.Search.Checks;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeIndexFindCursorParameters
	{
		public static BTreeIndexFindCursorParameters GetFindExactParameters<T>(T searchValue)
		{
			var search = searchValue switch
			{
				NormalisedValue nv => nv,
				_ => NormalisedValue.Create(searchValue)
			};

			var check = new KeyCheckBetweenInclusive(search, search);
			return new(-1, true, true, check);
		}

		public static BTreeIndexFindCursorParameters GetFindParameters(BarbadosDocument condition)
		{
			static void _throwInvalidCondition(bool notFalse)
			{
				if (notFalse)
				{
					throw new ArgumentException(
						"Expected a single condition with an optional '?inclusive' for a non-range condition",
						nameof(condition)
					);
				}
			}

			static NormalisedValue _retrieveSearchKey(BarbadosDocument condition)
			{
				if (!condition.TryGetNormalisedValue(BarbadosDocumentKeys.IndexQuery.SearchValue, out var value))
				{
					throw new ArgumentException(
						$"Expected a {BarbadosDocumentKeys.IndexQuery.SearchValue} for a non-range condition",
						nameof(condition)
					);
				}

				return value;
			}

			var doExact = condition.TryGetBoolean(BarbadosDocumentKeys.IndexQuery.Exact, out var ex) && ex;
			var doRange = condition.TryGetBoolean(BarbadosDocumentKeys.IndexQuery.Range, out var rg) && rg;
			var doLess = condition.TryGetBoolean(BarbadosDocumentKeys.IndexQuery.LessThan, out var lt) && lt;
			var doGreater = condition.TryGetBoolean(BarbadosDocumentKeys.IndexQuery.GreaterThan, out var gt) && gt;
			var doInclusive = condition.TryGetBoolean(BarbadosDocumentKeys.IndexQuery.Inclusive, out var incl) && incl;
			var doAsc = condition.TryGetBoolean(BarbadosDocumentKeys.IndexQuery.Ascending, out var asc) && asc;
			var doTake = condition.TryGetInt64(BarbadosDocumentKeys.IndexQuery.Take, out var take);

			if (doTake && take < 0)
			{
				throw new ArgumentException("'Take' argument must be a positive integer", nameof(condition));
			}

			KeyCheckRange check;
			if (doRange)
			{
				if (
					!condition.TryGetNormalisedValue(BarbadosDocumentKeys.IndexQuery.LessThan, out var end) ||
					!condition.TryGetNormalisedValue(BarbadosDocumentKeys.IndexQuery.GreaterThan, out var start)
				)
				{
					throw new ArgumentException(
						$"Could not find range bounds in '{BarbadosDocumentKeys.IndexQuery.LessThan}' " +
						$"and '{BarbadosDocumentKeys.IndexQuery.GreaterThan}' fields",
						nameof(condition)
					);
				}

				check = doInclusive
					? new KeyCheckBetweenInclusive(start, end)
					: new KeyCheckBetween(start, end);
			}

			else
			if (doExact || !(doLess || doGreater || doRange))
			{
				_throwInvalidCondition(doLess || doGreater || doRange);
				var search = _retrieveSearchKey(condition);
				check = new KeyCheckBetweenInclusive(search, search);
			}

			else
			if (doLess)
			{
				_throwInvalidCondition(doExact || doGreater || doRange);
				var search = _retrieveSearchKey(condition);
				check = doInclusive
					? new KeyCheckBetweenInclusive(NormalisedValue.Min, search)
					: new KeyCheckBetween(NormalisedValue.Min, search);
			}

			else
			if (doGreater)
			{
				_throwInvalidCondition(doExact || doLess || doRange);
				var search = _retrieveSearchKey(condition);
				check = doInclusive
					? new KeyCheckBetweenInclusive(search, NormalisedValue.Max)
					: new KeyCheckBetween(search, NormalisedValue.Max);
			}

			else
			{
				throw new ArgumentException("Unexpected condition", nameof(condition));
			}

			return new(take, !doTake, doAsc, check);
		}
	}
}
