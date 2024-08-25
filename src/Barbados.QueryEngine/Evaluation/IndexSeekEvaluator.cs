using System.Collections.Generic;

using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class IndexSeekEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children => [];

		private readonly ValueSelector _selector;
		private readonly BarbadosDocument _condition;
		private readonly IReadOnlyBTreeIndex _index;
		private readonly IReadOnlyBarbadosCollection _collection;

		public IndexSeekEvaluator(
			ValueSelector selector,
			BarbadosDocument condition,
			IReadOnlyBTreeIndex index,
			IReadOnlyBarbadosCollection collection
		)
		{
			_selector = selector;
			_condition = condition;
			_index = index;
			_collection = collection;
		}

		public IEnumerable<BarbadosDocument> Evaluate()
		{
			var cursor = _index.Find(_condition);
			foreach (var id in cursor)
			{
				if (!_collection.TryRead(id, _selector, out var document))
				{
					cursor.Close();
					throw new BarbadosInternalErrorException();
				}

				yield return document;
			}
		}

		public override string ToString() => FormatHelpers.FormatValueSelector($"IndexSeek({_index.IndexField})", _selector);
	}
}
