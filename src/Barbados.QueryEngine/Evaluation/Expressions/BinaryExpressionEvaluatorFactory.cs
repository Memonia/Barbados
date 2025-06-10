using System;

using Barbados.Documents;
using Barbados.QueryEngine.Build.Expressions;

namespace Barbados.QueryEngine.Evaluation.Expressions
{
	internal static class BinaryExpressionEvaluatorFactory
	{
		private sealed class AndExpressionEvaluator(
			BinaryExpression expression,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right,
			BarbadosDocument.Builder resultBuilder
		) : BinaryExpressionEvaluator(expression, left, right)
		{
			private readonly BarbadosDocument.Builder _resultBuilder = resultBuilder;

			protected override BarbadosDocument Evaluate(BarbadosDocument left, BarbadosDocument right)
			{
				var result =
					left.TryGetBoolean(QueryValueNames.Predicate, out var b1) &&
					right.TryGetBoolean(QueryValueNames.Predicate, out var b2) &&
					b1 && b2;

				return _resultBuilder.Add(QueryValueNames.Predicate, result).Build(true);
			}
		}

		private sealed class OrExpressionEvaluator(
			BinaryExpression expression,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right,
			BarbadosDocument.Builder resultBuilder
		) : BinaryExpressionEvaluator(expression, left, right)
		{
			private readonly BarbadosDocument.Builder _resultBuilder = resultBuilder;

			protected override BarbadosDocument Evaluate(BarbadosDocument left, BarbadosDocument right)
			{
				var result =
					left.TryGetBoolean(QueryValueNames.Predicate, out var b1) &&
					right.TryGetBoolean(QueryValueNames.Predicate, out var b2) &&
					(b1 || b2);

				return _resultBuilder.Add(QueryValueNames.Predicate, result).Build(true);
			}
		}

		public static BinaryExpressionEvaluator Create(
			BinaryExpression expression,
			BinaryOperator op,
			IQueryExpressionEvaluator left,
			IQueryExpressionEvaluator right
		)
		{
			return op switch
			{
				BinaryOperator.Or => new OrExpressionEvaluator(expression, left, right, new()),
				BinaryOperator.And => new AndExpressionEvaluator(expression, left, right, new()),
				_ => throw new NotImplementedException(),
			};
		}
	}
}
