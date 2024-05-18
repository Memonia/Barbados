using System;

using Barbados.QueryEngine.Build;
using Barbados.StorageEngine;

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

		public QueryPlanBuilder() : this(ValueSelector.SelectAll)
		{

		}

		public QueryPlanBuilder(ValueSelector selector)
		{
			Plan = new Scan(selector);
		}

		public QueryPlanBuilder Filter(IQueryExpression expr)
		{
			Plan = new Filter(Plan, expr);
			return this;
		}

		public QueryPlanBuilder Project(ValueSelector selector)
		{
			Plan = new Projection(Plan, selector);
			return this;
		}
	}
}
