using System;

using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Build.Expressions
{
	internal static class ComparisonExpressionFactory
	{
		private sealed class EqualsExpression(
			BarbadosIdentifier comparedField, IQueryExpression left, IQueryExpression right
		) : ComparisonExpression(comparedField, BinaryOperator.Equals, "Equals", "==", left, right)
		{ }

		private sealed class NotEqualsExpression(
			BarbadosIdentifier comparedField, IQueryExpression left, IQueryExpression right
		) : ComparisonExpression(comparedField, BinaryOperator.NotEquals, "NotEquals", "!=", left, right)
		{ }

		private sealed class LessThanExpression(
			BarbadosIdentifier comparedField, IQueryExpression left, IQueryExpression right
		) : ComparisonExpression(comparedField, BinaryOperator.LessThan, "LessThan", "<", left, right)
		{ }

		private sealed class LessThanOrEqualExpression(
			BarbadosIdentifier comparedField, IQueryExpression left, IQueryExpression right
		) : ComparisonExpression(comparedField, BinaryOperator.LessThanOrEqual, "LessThanOrEqual", "<=", left, right)
		{ }

		private sealed class GreaterThanExpression(
			BarbadosIdentifier comparedField, IQueryExpression left, IQueryExpression right
		) : ComparisonExpression(comparedField, BinaryOperator.GreaterThan, "GreaterThan", ">", left, right)
		{ }

		private sealed class GreaterThanOrEqualExpression(
			BarbadosIdentifier comparedField, IQueryExpression left, IQueryExpression right
		) : ComparisonExpression(comparedField, BinaryOperator.GreaterThanOrEqual, "GreaterThanOrEqual", ">=", left, right)
		{ }

		public static ComparisonExpression Create(
			BarbadosIdentifier comparedField,
			BinaryOperator op,
			IQueryExpression left,
			IQueryExpression right
		)
		{
			return op switch
			{
				BinaryOperator.Equals => new EqualsExpression(comparedField, left, right),
				BinaryOperator.NotEquals => new NotEqualsExpression(comparedField, left, right),
				BinaryOperator.LessThan => new LessThanExpression(comparedField, left, right),
				BinaryOperator.LessThanOrEqual => new LessThanOrEqualExpression(comparedField, left, right),
				BinaryOperator.GreaterThan => new GreaterThanExpression(comparedField, left, right),
				BinaryOperator.GreaterThanOrEqual => new GreaterThanOrEqualExpression(comparedField, left, right),
				_ => throw new NotImplementedException()
			};
		}
	}
}
