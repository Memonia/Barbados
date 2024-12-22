namespace Barbados.QueryEngine.Build.Expressions
{
	internal class ComparisonExpression(
		string comparedField,
		BinaryOperator op,
		string name,
		string symbol,
		IQueryExpression left,
		IQueryExpression right
	) : BinaryExpression(op, name, symbol, left, right)
	{
		public string ComparedField { get; } = comparedField;
	}
}
