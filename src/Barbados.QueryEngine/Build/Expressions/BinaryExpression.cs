namespace Barbados.QueryEngine.Build.Expressions
{
	internal abstract class BinaryExpression(
		BinaryOperator op,
		string name,
		string symbol,
		IQueryExpression left,
		IQueryExpression right
	) : IQueryExpression
	{
		public BinaryOperator Operator { get; } = op;
		public string Name { get; } = name;
		public string Symbol { get; } = symbol;
		public IQueryExpression Left { get; } = left;
		public IQueryExpression Right { get; } = right;

		public override string ToString() => $"({Left} {Symbol} {Right})";
	}
}
