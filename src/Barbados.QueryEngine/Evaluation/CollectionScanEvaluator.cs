using System.Collections.Generic;

using Barbados.Documents;
using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class CollectionScanEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children => [];

		private readonly BarbadosKeySelector _selector;
		private readonly IReadOnlyBarbadosCollection _collection;

		public CollectionScanEvaluator(IReadOnlyBarbadosCollection collection, BarbadosKeySelector selector)
		{
			_selector = selector;
			_collection = collection;
		}

		public IEnumerable<BarbadosDocument> Evaluate()
		{
			return _collection.GetCursor(_selector);
		}

		public override string ToString() => FormatHelpers.FormatValueSelector($"Scan({_collection.Name})", _selector);
	}
}
