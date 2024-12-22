using System.Collections.Generic;

using Barbados.Documents;

namespace Barbados.QueryEngine
{
	internal interface IQueryPlanEvaluator
	{
		IReadOnlyCollection<IQueryPlanEvaluator> Children { get; }

		IEnumerable<BarbadosDocument> Evaluate();
	}
}
