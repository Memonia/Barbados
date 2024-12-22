using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Indexing.Extensions;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal abstract class AbstractCollectionFacade
	{
		public ObjectId Id { get; }
		public BTreeClusteredIndexFacade ClusteredIndexFacade { get; }

		protected TransactionManager TransactionManager { get; }

		public AbstractCollectionFacade(
			ObjectId id,
			TransactionManager transactionManager,
			BTreeClusteredIndexFacade clusteredIndexFacade
		)
		{
			Id = id;
			TransactionManager = transactionManager;
			ClusteredIndexFacade = clusteredIndexFacade;
		}

		protected abstract IEnumerable<BTreeIndexFacade> EnumerateIndexes();

		public void Deallocate()
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			foreach (var index in EnumerateIndexes())
			{
				index.Deallocate(tx);
			}

			ClusteredIndexFacade.Deallocate(tx);
			TransactionManager.CommitTransaction(tx);
		}

		public void BTreeIndexBuild(BTreeIndexFacade facade)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var proxy = facade.GetProxy(tx);
			foreach (var document in GetCursor(facade.KeySelector))
			{
				if (document.TryGetNormalisedValue(facade.IndexField, out var value))
				{
					var ikey = facade.ToBTreeIndexKey(value);
					proxy.Insert(ikey, document.GetObjectId());
				}
			}

			TransactionManager.CommitTransaction(tx);
		}

		public bool TryRead(ObjectId id, BarbadosKeySelector selector, out BarbadosDocument document)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.Read);
			var proxy = ClusteredIndexFacade.GetProxy(tx);
			var idn = new ObjectIdNormalised(id);
			if (proxy.TryReadDocument(idn, selector, out document))
			{
				return true;
			}

			document = default!;
			return false;
		}

		public bool TryUpdate(ObjectId id, BarbadosDocument document)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var proxy = ClusteredIndexFacade.GetProxy(tx);
			if (!_tryRemove(proxy, id))
			{
				return false;
			}

			_insert(proxy, id, document);
			TransactionManager.CommitTransaction(tx);
			return true;
		}

		public bool TryRemove(ObjectId id)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var proxy = ClusteredIndexFacade.GetProxy(tx);
			if (_tryRemove(proxy, id))
			{
				TransactionManager.CommitTransaction(tx);
				return true;
			}

			return false;
		}

		public ObjectId Insert(BarbadosDocument document)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var collection = tx.Load<CollectionPage>(ClusteredIndexFacade.Info.RootHandle);
			if (collection.TryGetNextObjectId(out var nextId))
			{
				// Save next available document id
				tx.Save(collection);

				var proxy = ClusteredIndexFacade.GetProxy(tx);
				_insert(proxy, nextId, document);

				TransactionManager.CommitTransaction(tx);
				return nextId;
			}

			else
			{
				throw new BarbadosException(BarbadosExceptionCode.MaxDocumentCountReached);
			}
		}

		public CollectionCursor GetCursor()
		{
			return GetCursor(BarbadosKeySelector.SelectAll);
		}

		public CollectionCursor GetCursor(BarbadosKeySelector selector)
		{
			return new CollectionCursor(Id, TransactionManager, selector, ClusteredIndexFacade);
		}

		private void _insert(BTreeClusteredIndexTransactionProxy proxy, ObjectId id, BarbadosDocument document)
		{
			var idn = new ObjectIdNormalised(id);
			proxy.Insert(idn, document.AsBytes());
			foreach (var indexFacade in EnumerateIndexes())
			{
				var iproxy = indexFacade.GetProxy(proxy.Transaction);
				if (document.TryGetNormalisedValue(iproxy.Info.IndexField, out var value))
				{
					var ikey = indexFacade.ToBTreeIndexKey(value);
					iproxy.Insert(ikey, id);
				}
			}
		}

		private bool _tryRemove(BTreeClusteredIndexTransactionProxy proxy, ObjectId id)
		{
			var idn = new ObjectIdNormalised(id);
			if (!proxy.TryReadDocument(idn, BarbadosKeySelector.SelectAll, out var buffer))
			{
				return false;
			}

			if (!proxy.TryRemove(idn))
			{
				throw new BarbadosInternalErrorException();
			}

			foreach (var indexFacade in EnumerateIndexes())
			{
				var iproxy = indexFacade.GetProxy(proxy.Transaction);
				if (buffer.TryGetNormalisedValue(iproxy.Info.IndexField, out var value))
				{
					var ikey = indexFacade.ToBTreeIndexKey(value);
					if (!iproxy.TryRemove(ikey, id))
					{
						throw new BarbadosInternalErrorException();
					}
				}
			}

			return true;
		}
	}
}
