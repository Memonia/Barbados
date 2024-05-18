using Barbados.QueryEngine.Build.Expressions;
using Barbados.StorageEngine;
using Barbados.StorageEngine.Documents;

namespace Barbados.QueryEngine.Evaluation.Expressions
{
	internal sealed class FieldExpressionEvaluator(FieldExpression expression) : IQueryExpressionEvaluator
	{
		public IQueryExpression Expression { get; } = expression;

		private readonly BarbadosIdentifier _name = expression.Name;

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
