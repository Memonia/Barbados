using System;

using Barbados.Documents;
using Barbados.QueryEngine.Build;

namespace Barbados.QueryEngine
{
	internal sealed class QueryPlanBuilder
	{
		public IQueryPlan Plan { get; private set; }

		private bool _built = false;

		public IQueryPlan Build()
		{
			if (_built)
			{
				throw new InvalidOperationException("Cannot reuse builder instance");
			}

			_built = true;
			return Plan;
		}

		public QueryPlanBuilder() : this(BarbadosKeySelector.SelectAll)
		{

		}

		public QueryPlanBuilder(BarbadosKeySelector selector)
		{
			Plan = new Scan(selector);
		}

		public QueryPlanBuilder Filter(IQueryExpression expr)
		{
			Plan = new Filter(Plan, expr);
			return this;
		}

		public QueryPlanBuilder Project(BarbadosKeySelector selector)
		{
			Plan = new Projection(Plan, selector);
			return this;
		}
	}
}
