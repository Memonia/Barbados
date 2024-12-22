using System.Collections.Generic;

using Barbados.Documents;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class FilterEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children { get; }
		public IQueryExpressionEvaluator Expression { get; }

		public FilterEvaluator(IReadOnlyCollection<IQueryPlanEvaluator> input, IQueryExpressionEvaluator expression)
		{
			Children = input;
			Expression = expression;
		}

		public IEnumerable<BarbadosDocument> Evaluate()
		{
			foreach (var child in Children)
			{
				foreach (var document in child.Evaluate())
				{
					var result = Expression.Evaluate(document);
					if (result.TryGetBoolean(QueryValueNames.Predicate, out var boolean) && boolean)
					{
						yield return document;
					}
				}
			}
		}

		public override string ToString() => $"Filter: {Expression}";
	}
}
