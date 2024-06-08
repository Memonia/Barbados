using System.Collections.Concurrent;
using System.Diagnostics;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Collections.Internal;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine
{
	internal sealed partial class BarbadosController : IBarbadosController
	{
		public PagePool Pool { get; }
		public LockManager Lock { get; }

		private readonly object _sync;
		private readonly ConcurrentDictionary<string, AbstractCollection> _instances;

		public BarbadosController(PagePool pool, LockManager lockManager)
		{
			Pool = pool;
			Lock = lockManager;

			_sync = new();
			_instances = [];
		}

		public BTreeIndex GetIndex(BarbadosIdentifier collection, BarbadosIdentifier field)
		{
			if (!TryGetIndex(collection, field, out var index))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.IndexDoesNotExist,
					$"Index on '{field}' in collection '{collection}' does not exist"
				);
			}

			return index;
		}

		public BarbadosCollection GetCollection(string name)
		{
			if (!TryGetCollection(name, out var collection))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.CollectionDoesNotExist, $"Collection '{name}' does not exist"
				);
			}

			return collection;
		}

		public bool TryGetIndex(BarbadosIdentifier collection, BarbadosIdentifier field, out BTreeIndex index)
		{
			lock (_sync)
			{
				var instance = GetCollection(collection);
				return instance.TryGetBTreeIndex(field, out index);
			}
		}

		public bool TryGetCollection(string name, out BarbadosCollection collection)
		{
			if (_instances.TryGetValue(name, out var instance))
			{
				collection = (BarbadosCollection)instance;
				return true;
			}

			lock (_sync)
			{
				if (_instances.TryGetValue(name, out instance))
				{
					collection = (BarbadosCollection)instance;
					return true;
				}

				var meta = GetMetaCollection();
				if (!meta.Find(name, out var document))
				{
					collection = default!;
					return false;
				}

				Lock.AddLockable(name);
				var @lock = Lock.GetLock(name);

				collection = MetaCollection.CreateCollectionInstance(document, Pool, @lock);
				foreach (var indexDocument in MetaCollection.GetIndexDocuments(document))
				{
					collection.AddBTreeIndex(
						MetaCollection.CreateIndexInstance(
							document, Pool, @lock, collection.ClusteredIndex
						)
					);
				}

				var r = _instances.TryAdd(name, collection);
				Debug.Assert(r);
				return true;
			}
		}

		public MetaCollection GetMetaCollection()
		{
			if (_instances.TryGetValue(BarbadosIdentifiers.Collection.MetaCollection, out var instance))
			{
				return (MetaCollection)instance;
			}

			lock (_sync)
			{
				if (_instances.TryGetValue(BarbadosIdentifiers.Collection.MetaCollection, out instance))
				{
					return (MetaCollection)instance;
				}

				Lock.AddLockable(BarbadosIdentifiers.Collection.MetaCollection);

				var root = Pool.LoadPin<RootPage>(PageHandle.Root);
				var metaLock = Lock.GetLock(BarbadosIdentifiers.Collection.MetaCollection);

				var clusteredIndex = new BTreeClusteredIndex(Pool, root.MetaCollectionClusteredIndexRootPageHandle);
				var index = new BTreeIndex(
					new()
					{
						IndexedField = BarbadosIdentifiers.MetaCollection.CollectionDocumentNameFieldAbsolute,
						KeyMaxLength = MetaCollection.NameIndexKeyMaxLength,
						RootPageHandle = root.MetaCollectionNameIndexRootPageHandle
					},
					metaLock,
					clusteredIndex,
					Pool
				);

				var meta = new MetaCollection(root.MetaCollectionPageHandle, Pool, metaLock, index, clusteredIndex);
				var r = _instances.TryAdd(BarbadosIdentifiers.Collection.MetaCollection, meta);
				Debug.Assert(r);

				Pool.Release(root);
				return meta;
			}
		}
	}
}
