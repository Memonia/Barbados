using System.Collections.Generic;

using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Documents;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class CollectionScanEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children => [];

		private readonly ValueSelector _selector;
		private readonly IReadOnlyBarbadosCollection _collection;

		public CollectionScanEvaluator(IReadOnlyBarbadosCollection collection, ValueSelector selector)
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
