namespace Barbados.QueryEngine.Query
{
	public sealed class Filter
	{
		internal IQueryExpression Expression { get; }

		internal Filter(IQueryExpression expression)
		{
			Expression = expression;
		}
	}
}
