using Barbados.Documents;

namespace Barbados.QueryEngine.Evaluation.Expressions
{
	internal abstract class BinaryExpressionEvaluator(
		IQueryExpression expression,
		IQueryExpressionEvaluator left, IQueryExpressionEvaluator right
	) : IQueryExpressionEvaluator
	{
		public IQueryExpression Expression { get; } = expression;

		private readonly IQueryExpressionEvaluator _left = left;
		private readonly IQueryExpressionEvaluator _right = right;

		public BarbadosDocument Evaluate(BarbadosDocument document)
		{
			return Evaluate(
				_left.Evaluate(document), _right.Evaluate(document)
			);
		}

		protected abstract BarbadosDocument Evaluate(BarbadosDocument left, BarbadosDocument right);

		public override string ToString() => Expression.ToString()!;
	}
}
