
using Barbados.QueryEngine.Build.Expressions;

namespace Barbados.QueryEngine.Query
{
	public static class QueryBuilder
	{
		public static class Filters
		{
			public static Filter Or(Filter left, Filter right)
			{
				return new Filter(
					BinaryExpressionFactory.Create(
						BinaryOperator.Or,
						left.Expression,
						right.Expression
					)
				);
			}

			public static Filter And(Filter left, Filter right)
			{
				return new Filter(
					BinaryExpressionFactory.Create(
						BinaryOperator.And,
						left.Expression,
						right.Expression
					)
				);
			}

			public static Filter Not(Filter filter)
			{
				return new Filter(
					new NotExpression(filter.Expression)
				);
			}

			public static Filter Eq<T>(string field, T value)
			{
				return new Filter(
					ComparisonExpressionFactory.Create(
						field,
						BinaryOperator.Equals,
						new FieldExpression(field),
						new ConstantExpression<T>(value, field)
					)
				);
			}

			public static Filter Neq<T>(string field, T value)
			{
				return new Filter(
					ComparisonExpressionFactory.Create(
						field,
						BinaryOperator.NotEquals,
						new FieldExpression(field),
						new ConstantExpression<T>(value, field)
					)
				);
			}

			public static Filter Lt<T>(string field, T value)
			{
				return new Filter(
					ComparisonExpressionFactory.Create(
						field,
						BinaryOperator.LessThan,
						new FieldExpression(field),
						new ConstantExpression<T>(value, field)
					)
				);
			}

			public static Filter LtEq<T>(string field, T value)
			{
				return new Filter(
					ComparisonExpressionFactory.Create(
						field,
						BinaryOperator.LessThanOrEqual,
						new FieldExpression(field),
						new ConstantExpression<T>(value, field)
					)
				);
			}

			public static Filter Gt<T>(string field, T value)
			{
				return new Filter(
					ComparisonExpressionFactory.Create(
						field,
						BinaryOperator.GreaterThan,
						new FieldExpression(field),
						new ConstantExpression<T>(value, field)
					)
				);
			}

			public static Filter GtEq<T>(string field, T value)
			{
				return new Filter(
					ComparisonExpressionFactory.Create(
						field,
						BinaryOperator.GreaterThanOrEqual,
						new FieldExpression(field),
						new ConstantExpression<T>(value, field)
					)
				);
			}
		}

		public static Projection Projection => new();
	}
}
