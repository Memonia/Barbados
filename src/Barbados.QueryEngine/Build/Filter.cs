namespace Barbados.QueryEngine.Build
{
	internal sealed class Filter(IQueryPlan input, IQueryExpression expr) : IQueryPlan
	{
		public IQueryPlan Child { get; } = input;
		public IQueryExpression Expression { get; } = expr;

		public override string ToString() => $"Filter: {Expression}";
	}
}
