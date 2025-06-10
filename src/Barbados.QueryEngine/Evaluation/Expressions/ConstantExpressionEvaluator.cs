using Barbados.Documents;

namespace Barbados.QueryEngine.Evaluation.Expressions
{
	internal sealed class ConstantExpressionEvaluator(
		IQueryExpression expression, BarbadosDocument value
	) : IQueryExpressionEvaluator
	{
		public IQueryExpression Expression { get; } = expression;

		private readonly BarbadosDocument _value = value;

		public BarbadosDocument Evaluate(BarbadosDocument document) => _value;

		public override string ToString() => Expression.ToString()!;
	}
}
