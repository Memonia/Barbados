using System.Collections.Generic;

using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine;
using Barbados.StorageEngine.Documents;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class ProjectionEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children { get; }

		private readonly ValueSelector _selector;
		private readonly BarbadosDocument.Builder _evaluationResultBuilder;

		public ProjectionEvaluator(
			IReadOnlyCollection<IQueryPlanEvaluator> input,
			ValueSelector selector,
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
					foreach (var value in _selector)
					{
						if (value.IsDocument && document.HasDocument(value))
						{
							_evaluationResultBuilder.AddDocumentFrom(value, document);
						}

						else
						if (document.HasField(value))
						{
							_evaluationResultBuilder.AddFieldFrom(value, document);
						}
					}

					yield return _evaluationResultBuilder.Build(reset: true);
				}
			}
		}

		public override string ToString() => FormatHelpers.FormatValueSelector("Projection", _selector);
	}
}
