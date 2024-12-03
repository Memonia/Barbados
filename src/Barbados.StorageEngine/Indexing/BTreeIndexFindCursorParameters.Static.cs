using System;

using Barbados.StorageEngine.Documents;
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
				if (
					!condition.Buffer.TryGetNormalisedValue(
						CommonIdentifiers.Index.SearchValue.BinaryName, out var value
					)
				)
				{
					throw new ArgumentException(
						$"Expected a {CommonIdentifiers.Index.SearchValue} for a non-range condition",
						nameof(condition)
					);
				}

				return value;
			}

			var doExact = condition.Buffer.TryGetBoolean(CommonIdentifiers.Index.Exact.BinaryName.AsBytes(), out var ex) && ex;
			var doRange = condition.Buffer.TryGetBoolean(CommonIdentifiers.Index.Range.BinaryName.AsBytes(), out var rg) && rg;
			var doLess = condition.Buffer.TryGetBoolean(CommonIdentifiers.Index.LessThan.BinaryName.AsBytes(), out var lt) && lt;
			var doGreater = condition.Buffer.TryGetBoolean(CommonIdentifiers.Index.GreaterThan.BinaryName.AsBytes(), out var gt) && gt;
			var doInclusive = condition.Buffer.TryGetBoolean(CommonIdentifiers.Index.Inclusive.BinaryName.AsBytes(), out var incl) && incl;
			var doAsc = condition.Buffer.TryGetBoolean(CommonIdentifiers.Index.Ascending.BinaryName.AsBytes(), out var asc) && asc;
			var doTake = condition.Buffer.TryGetInt64(CommonIdentifiers.Index.Take.BinaryName.AsBytes(), out var take);

			if (doTake && take < 0)
			{
				throw new ArgumentException("'Take' argument must be a positive integer", nameof(condition));
			}

			KeyCheckRange check;
			if (doRange)
			{
				if (
					!condition.Buffer.TryGetNormalisedValue(
						CommonIdentifiers.Index.LessThan.BinaryName, out var end
					) ||
					!condition.Buffer.TryGetNormalisedValue(
						CommonIdentifiers.Index.GreaterThan.BinaryName, out var start
					)
				)
				{
					throw new ArgumentException(
						$"Could not find range bounds in '{CommonIdentifiers.Index.LessThan}' " +
						$"and '{CommonIdentifiers.Index.GreaterThan}' fields",
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

			return new(take, !doTake, asc, check);
		}

	}
}
