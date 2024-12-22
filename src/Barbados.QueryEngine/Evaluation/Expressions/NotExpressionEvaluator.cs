using Barbados.Documents;
using Barbados.QueryEngine.Build.Expressions;

namespace Barbados.QueryEngine.Evaluation.Expressions
{
	internal sealed class NotExpressionEvaluator(
		NotExpression expression,
		IQueryExpressionEvaluator input,
		BarbadosDocument.Builder resultBuilder
	) : IQueryExpressionEvaluator
	{
		public IQueryExpression Expression { get; } = expression;

		private readonly IQueryExpressionEvaluator _evaluator = input;
		private readonly BarbadosDocument.Builder _resultBuilder = resultBuilder;

		public BarbadosDocument Evaluate(BarbadosDocument document)
		{
			var result = _evaluator.Evaluate(document);
			if (result.TryGetBoolean(QueryValueNames.Predicate, out var boolean))
			{
				return _resultBuilder
					.Add(QueryValueNames.Predicate, !boolean)
					.Build(true);
			}

			return _resultBuilder
				.Add(QueryValueNames.Predicate, false)
				.Build(true);
		}

		public override string ToString() => Expression.ToString()!;
	}
}
