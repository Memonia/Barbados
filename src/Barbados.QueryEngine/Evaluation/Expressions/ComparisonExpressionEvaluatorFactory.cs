using System;

using Barbados.Documents;
using Barbados.QueryEngine.Build.Expressions;

namespace Barbados.QueryEngine.Evaluation.Expressions
{
	internal static class ComparisonExpressionEvaluatorFactory
	{
		private sealed class EqualsExpressionEvaluator(
			ComparisonExpression expression,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right,
			string comparedField,
			BarbadosDocument.Builder resultBuilder
		) : ComparisonExpressionEvaluator(expression, left, right, comparedField, resultBuilder)
		{
			protected override bool InterpretResult(int result) => result == 0;
		}

		private sealed class NotEqualsExpressionEvaluator(
			ComparisonExpression expression,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right,
			string comparedField,
			BarbadosDocument.Builder resultBuilder
		) : ComparisonExpressionEvaluator(expression, left, right, comparedField, resultBuilder)
		{
			protected override bool InterpretResult(int result) => result != 0;
		}

		private sealed class LessThanExpressionEvaluator(
			ComparisonExpression expression,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right,
			string comparedField,
			BarbadosDocument.Builder resultBuilder
		) : ComparisonExpressionEvaluator(expression, left, right, comparedField, resultBuilder)
		{
			protected override bool InterpretResult(int result) => result < 0;
		}

		private sealed class LessThanOrEqualExpressionEvaluator(
			ComparisonExpression expression,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right,
			string comparedField,
			BarbadosDocument.Builder resultBuilder
		) : ComparisonExpressionEvaluator(expression, left, right, comparedField, resultBuilder)
		{
			protected override bool InterpretResult(int result) => result <= 0;
		}

		private sealed class GreaterThanExpressionEvaluator(
			ComparisonExpression expression,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right,
			string comparedField,
			BarbadosDocument.Builder resultBuilder
		) : ComparisonExpressionEvaluator(expression, left, right, comparedField, resultBuilder)
		{
			protected override bool InterpretResult(int result) => result > 0;
		}

		private sealed class GreaterThanOrEqualExpressionEvaluator(
			ComparisonExpression expression,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right,
			string comparedField,
			BarbadosDocument.Builder resultBuilder
		) : ComparisonExpressionEvaluator(expression, left, right, comparedField, resultBuilder)
		{
			protected override bool InterpretResult(int result) => result >= 0;
		}

		public static ComparisonExpressionEvaluator Create(
			ComparisonExpression expression,
			string comparedField,
			BinaryOperator op,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right
		)
		{
			return op switch
			{
				BinaryOperator.Equals => new EqualsExpressionEvaluator(expression, left, right, comparedField, new()),
				BinaryOperator.NotEquals => new NotEqualsExpressionEvaluator(expression, left, right, comparedField, new()),
				BinaryOperator.LessThan => new LessThanExpressionEvaluator(expression, left, right, comparedField, new()),
				BinaryOperator.GreaterThan => new GreaterThanExpressionEvaluator(expression, left, right, comparedField, new()),
				BinaryOperator.LessThanOrEqual => new LessThanOrEqualExpressionEvaluator(expression, left, right, comparedField, new()),
				BinaryOperator.GreaterThanOrEqual => new GreaterThanOrEqualExpressionEvaluator(expression, left, right, comparedField, new()),
				_ => throw new NotImplementedException(),
			};
		}
	}
}
