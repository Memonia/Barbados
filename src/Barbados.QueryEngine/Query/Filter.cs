namespace Barbados.QueryEngine.Query
{
	internal sealed class Filter(IQueryExpression expression) : IFilter
	{
		public IQueryExpression Expression { get; } = expression;
	}
}
