
using Barbados.QueryEngine.Build.Expressions;
using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Query
{
	public static class QueryBuilder
	{
		public static class Filters
		{
			public static IFilter Or(IFilter left, IFilter right)
			{
				return new Filter(
					BinaryExpressionFactory.Create(
						BinaryOperator.Or,
						left.Expression,
						right.Expression
					)
				);
			}

			public static IFilter And(IFilter left, IFilter right)
			{
				return new Filter(
					BinaryExpressionFactory.Create(
						BinaryOperator.And,
						left.Expression,
						right.Expression
					)
				);
			}

			public static IFilter Not(IFilter filter)
			{
				return new Filter(
					new NotExpression(filter.Expression)
				);
			}

			public static IFilter Eq<T>(BarbadosIdentifier field, T value)
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

			public static IFilter Neq<T>(BarbadosIdentifier field, T value)
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

			public static IFilter Lt<T>(BarbadosIdentifier field, T value)
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

			public static IFilter LtEq<T>(BarbadosIdentifier field, T value)
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

			public static IFilter Gt<T>(BarbadosIdentifier field, T value)
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

			public static IFilter GtEq<T>(BarbadosIdentifier field, T value)
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

		public static IProjection Projection => new Projection();
	}
}
