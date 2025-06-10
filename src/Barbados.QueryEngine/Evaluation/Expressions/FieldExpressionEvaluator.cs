using Barbados.Documents;
using Barbados.QueryEngine.Build.Expressions;

namespace Barbados.QueryEngine.Evaluation.Expressions
{
	internal sealed class FieldExpressionEvaluator(FieldExpression expression) : IQueryExpressionEvaluator
	{
		public IQueryExpression Expression { get; } = expression;

		private readonly BarbadosKey _name = expression.Name;

		public BarbadosDocument Evaluate(BarbadosDocument document)
		{
			if (document.TryGetWrapped(_name, out var extracted))
			{
				return extracted;
			}

			return BarbadosDocument.Empty;
		}

		public override string ToString() => Expression.ToString()!;
	}
}
