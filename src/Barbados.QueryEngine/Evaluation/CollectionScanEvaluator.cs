using System.Collections.Generic;

using Barbados.Documents;
using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class CollectionScanEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children => [];

		private readonly FindOptions _options;
		private readonly IReadOnlyBarbadosCollection _collection;

		public CollectionScanEvaluator(IReadOnlyBarbadosCollection collection, FindOptions options)
		{
			_options = options;
			_collection = collection;
		}

		public IEnumerable<BarbadosDocument> Evaluate()
		{
			using var cursor = _collection.Find(_options);
			foreach (var document in cursor)
			{
				yield return document;
			}
		}

		public override string ToString() => FormatHelpers.FormatSelection($"Scan({_collection.Name})", _options);
	}
}
