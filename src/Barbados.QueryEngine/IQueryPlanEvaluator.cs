using System.Collections.Generic;

using Barbados.StorageEngine.Documents;

namespace Barbados.QueryEngine
{
	internal interface IQueryPlanEvaluator
	{
		IReadOnlyCollection<IQueryPlanEvaluator> Children { get; }

		IEnumerable<BarbadosDocument> Evaluate();
	}
}
