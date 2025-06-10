namespace Barbados.QueryEngine.Build
{
	internal sealed class FilterPlan : IQueryPlan
	{
		public IQueryPlan Child { get; }
		public IQueryExpression Expression { get; }

		public FilterPlan(IQueryPlan input, IQueryExpression expr)
		{
			Child = input;
			Expression = expr;
		}

		public override string ToString() => $"Filter: {Expression}";
	}
}
