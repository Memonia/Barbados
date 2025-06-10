using System;
using System.Collections.Generic;

using Barbados.Documents;
using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Query
{
	public sealed class Query
	{
		private readonly QueryPlanBuilder _builder;
		private readonly IReadOnlyBarbadosCollection _collection;

		private IQueryPlanEvaluator? _translated;

		internal Query(IReadOnlyBarbadosCollection collection, QueryPlanBuilder builder)
		{
			_builder = builder;
			_collection = collection;
		}

		public Query Filter(Filter filter)
		{
			_throwTranslated();
			_builder.Filter(filter.Expression);
			return this;
		}

		public Query Project(Projection projection)
		{
			_throwTranslated();
			_builder.Project(projection.GetCurrentSelection());
			return this;
		}

		public string Format()
		{
			return FormatHelpers.FormatPlan(_builder.Plan);
		}

		public string FormatTranslated()
		{
			_ensureTranslated();
			return FormatHelpers.FormatPlanEvaluator(_translated!);
		}

		public IEnumerable<BarbadosDocument> Execute()
		{
			_ensureTranslated();
			return _translated!.Evaluate();
		}

		private void _throwTranslated()
		{
			if (_translated is not null)
			{
				throw new InvalidOperationException(
					"The query plan has been translated and cannot be modified anymore"
				);
			}
		}

		private void _ensureTranslated()
		{
			_translated ??= QueryTranslator.TranslatePlan(_builder.Build(), _collection);
		}
	}
}
