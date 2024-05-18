using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Build.Expressions
{
	internal class ComparisonExpression(
		BarbadosIdentifier comparedField,
		BinaryOperator op,
		string name,
		string symbol,
		IQueryExpression left,
		IQueryExpression right
	) : BinaryExpression(op, name, symbol, left, right)
	{
		public BarbadosIdentifier ComparedField { get; } = comparedField;
	}
}
