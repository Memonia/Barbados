using System.Collections.Generic;

using Barbados.Documents;
using Barbados.QueryEngine.Helpers;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class ProjectionEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children { get; }

		private readonly KeySelection _selection;
		private readonly BarbadosDocument.Builder _evaluationResultBuilder;

		public ProjectionEvaluator(
			IReadOnlyCollection<IQueryPlanEvaluator> input,
			KeySelection selection,
			BarbadosDocument.Builder evaluationResultBuilder
		)
		{
			Children = input;
			_selection = selection;
			_evaluationResultBuilder = evaluationResultBuilder;
		}

		public IEnumerable<BarbadosDocument> Evaluate()
		{
			foreach (var child in Children)
			{
				foreach (var document in child.Evaluate())
				{
					foreach (var key in _selection.Keys)
					{
						if (document.HasField(key))
						{
							_evaluationResultBuilder.AddFrom(key, document);
						}
					}

					yield return _evaluationResultBuilder.Build(reset: true);
				}
			}
		}

		public override string ToString() => FormatHelpers.FormatSelection("Projection", _selection);
	}
}
