using System.Collections.Generic;

using Barbados.Documents;
using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine.Collections;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class IndexSeekEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children => [];

		private readonly BarbadosKey _field;
		private readonly FindOptions _options;
		private readonly IReadOnlyBarbadosCollection _collection;

		public IndexSeekEvaluator(IReadOnlyBarbadosCollection collection, BarbadosKey field, FindOptions options)
		{
			_field = field;
			_options = options;
			_collection = collection;
		}

		public IEnumerable<BarbadosDocument> Evaluate()
		{
			using var cursor = _collection.Find(_options, _field);
			foreach (var document in cursor)
			{
				yield return document;
			}
		}

		public override string ToString() => FormatHelpers.FormatSelection($"IndexSeek({_field})", _options);
	}
}
