using System.Collections.Generic;

using Barbados.QueryEngine.Helpers;
using Barbados.StorageEngine;
using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.QueryEngine.Evaluation
{
	internal sealed class IndexSeekEvaluator : IQueryPlanEvaluator
	{
		public IReadOnlyCollection<IQueryPlanEvaluator> Children => [];

		private readonly ValueSelector _selector;
		private readonly BarbadosDocument _condition;
		private readonly IReadOnlyBTreeIndex _index;
		private readonly IBarbadosCollection _collection;

		public IndexSeekEvaluator(
			ValueSelector selector,
			BarbadosDocument condition,
			IReadOnlyBTreeIndex index,
			IBarbadosCollection collection
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
			foreach (var id in _index.Find(_condition))
			{
				if (!_collection.TryRead(id, _selector, out var document))
				{
					cursor.Close();
					throw new BarbadosException(BarbadosExceptionCode.InternalError);
				}

				yield return document;
			}
		}

		public override string ToString() => FormatHelpers.FormatValueSelector($"IndexSeek({_index.Name})", _selector);
	}
}
