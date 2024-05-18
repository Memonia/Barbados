using System;

namespace Barbados.QueryEngine.Build.Expressions
{
	internal static class BinaryExpressionFactory
	{
		private sealed class OrExpression(IQueryExpression left, IQueryExpression right) :
			BinaryExpression(BinaryOperator.Or, "Or", "||", left, right)
		{ }

		private sealed class AndExpression(IQueryExpression left, IQueryExpression right) :
			BinaryExpression(BinaryOperator.And, "And", "&&", left, right)
		{ }

		public static BinaryExpression Create(BinaryOperator op, IQueryExpression left, IQueryExpression right)
		{
			return op switch
			{
				BinaryOperator.Or => new OrExpression(left, right),
				BinaryOperator.And => new AndExpression(left, right),
				_ => throw new NotImplementedException()
			};
		}
	}
}
