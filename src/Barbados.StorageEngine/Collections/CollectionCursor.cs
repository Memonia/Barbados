using System.Collections.Generic;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class CollectionCursor : Cursor<BarbadosDocument>
	{
		private readonly ValueSelector _selector;
		private readonly BTreeClusteredIndexFacade _clusteredIndexFacade;

		public CollectionCursor(
			ObjectId collectionId,
			TransactionManager transactionManager,
			ValueSelector selector,
			BTreeClusteredIndexFacade clusteredIndexFacade
		) : base(collectionId, transactionManager)
		{
			_selector = selector;
			_clusteredIndexFacade = clusteredIndexFacade;
		}

		protected override IEnumerable<BarbadosDocument> EnumerateValues(TransactionScope transaction)
		{
			var proxy = _clusteredIndexFacade.GetProxy(transaction);
			if (!proxy.TryGetLeftmostLeafHandle(out var next))
			{
				yield break;
			}

			var docs = new List<BarbadosDocument>();
			while (!next.IsNull)
			{
				var page = transaction.Load<ObjectPage>(next);
				var e = page.GetEnumerator();
				while (e.TryGetNext(out var id))
				{
					var idn = new ObjectIdNormalised(id);
					if (!proxy.TryReadObjectBuffer(idn, page, _selector, out var buffer))
					{
						throw new BarbadosInternalErrorException();
					}

					docs.Add(new BarbadosDocument(id, buffer));
				}

				foreach (var doc in docs)
				{
					yield return doc;
				}

				docs.Clear();
				next = page.Next;
			}
		}
	}
}
