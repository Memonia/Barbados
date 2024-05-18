using Barbados.QueryEngine.Build.Expressions;
using Barbados.StorageEngine;
using Barbados.StorageEngine.Documents;

namespace Barbados.QueryEngine.Evaluation.Expressions
{
	internal abstract class ComparisonExpressionEvaluator(
		ComparisonExpression expression,
		IQueryExpressionEvaluator left,
		IQueryExpressionEvaluator right,
		BarbadosIdentifier comparedField,
		BarbadosDocument.Builder resultBuilder
		) : BinaryExpressionEvaluator(expression, left, right)
	{
		private readonly BarbadosIdentifier _comparedField = comparedField;
		private readonly BarbadosDocument.Builder _resultBuilder = resultBuilder;

		protected override BarbadosDocument Evaluate(BarbadosDocument left, BarbadosDocument right)
		{
			if (BarbadosDocument.TryCompareFields(_comparedField, left, right, out int result))
			{
				return _resultBuilder
					.Add(QueryValueNames.Predicate, InterpretResult(result))
					.Build(true);
			}

			return _resultBuilder
				.Add(QueryValueNames.Predicate, false)
				.Build(true);
		}

		protected abstract bool InterpretResult(int result);
	}
}
