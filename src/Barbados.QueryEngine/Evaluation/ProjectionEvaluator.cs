using System.Collections.Generic;

using Barbados.Documents;
using Barbados.QueryEngine.Helpers;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class ProjectionEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children { get; }

		private readonly BarbadosKeySelector _selector;
		private readonly BarbadosDocument.Builder _evaluationResultBuilder;

		public ProjectionEvaluator(
			IReadOnlyCollection<IQueryPlanEvaluator> input,
			BarbadosKeySelector selector,
			BarbadosDocument.Builder evaluationResultBuilder
		)
		{
			Children = input;
			_selector = selector;
			_evaluationResultBuilder = evaluationResultBuilder;
		}

		public IEnumerable<BarbadosDocument> Evaluate()
		{
			foreach (var child in Children)
			{
				foreach (var document in child.Evaluate())
				{
					foreach (var key in _selector)
					{
						_evaluationResultBuilder.AddFrom(key, document);
					}

					yield return _evaluationResultBuilder.Build(reset: true);
				}
			}
		}

		public override string ToString() => FormatHelpers.FormatValueSelector("Projection", _selector);
	}
}
