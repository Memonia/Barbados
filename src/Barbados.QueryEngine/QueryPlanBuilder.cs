using System;

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

		public QueryPlanBuilder() : this(new KeySelection([], true))
		{

		}

		public QueryPlanBuilder(KeySelection selection)
		{
			Plan = new ScanPlan(selection);
		}

		public QueryPlanBuilder Filter(IQueryExpression expr)
		{
			Plan = new FilterPlan(Plan, expr);
			return this;
		}

		public QueryPlanBuilder Project(KeySelection selection)
		{
			Plan = new ProjectionPlan(Plan, selection);
			return this;
		}
	}
}
