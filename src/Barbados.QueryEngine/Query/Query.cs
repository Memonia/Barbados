using System;
using System.Collections.Generic;

using Barbados.Documents;
using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Query
{
	internal sealed class Query(IReadOnlyBarbadosCollection collection, QueryPlanBuilder builder) : IQuery
	{
		private void _throwTranslated()
		{
			if (_translated is not null)
			{
				throw new InvalidOperationException(
					"The query plan has been translated and cannot be modified anymore"
				);
			}
		}

		private IQueryPlanEvaluator? _translated = null;

		private readonly QueryPlanBuilder _builder = builder;
		private readonly IReadOnlyBarbadosCollection collection = collection;

		public IQuery Filter(IFilter filter)
		{
			_throwTranslated();
			_builder.Filter(filter.Expression);
			return this;
		}

		public IQuery Project(IProjection projection)
		{
			_throwTranslated();
			_builder.Project(projection.GetSelector());
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

		private void _ensureTranslated()
		{
			_translated ??= QueryTranslator.TranslatePlan(_builder.Build(), collection);
		}
	}
}
