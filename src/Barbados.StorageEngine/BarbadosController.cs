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
		private LockManager Lock { get; }

		private readonly object _sync;
		private readonly ConcurrentDictionary<string, AbstractCollection> _instances;

		public BarbadosController(PagePool pool, LockManager lockManager)
		{
			Pool = pool;
			Lock = lockManager;

			_sync = new();
			_instances = [];
		}

		public ObjectLock GetLock(string name, LockMode mode)
		{
			return Lock.GetLock(name, mode);
		}

		public ObjectLock AcquireLock(string name, LockMode mode)
		{
			var @lock = GetLock(name, mode);
			@lock.Acquire();
			return @lock;
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
				foreach (var storedIndex in instance.Indexes)
				{
					if (storedIndex.IndexedField.Identifier == field.Identifier)
					{
						index = storedIndex;
						return true;
					}
				}

				index = default!;
				return false;
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

				collection = MetaCollection.CreateCollectionInstance(document, this);

				Lock.AddLockable(name);
				foreach (var index in collection.Indexes)
				{
					Lock.AddLockable(index.Name);
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

				var root = Pool.LoadPin<RootPage>(PageHandle.Root);
				var index = new BTreeIndex(
					BarbadosIdentifiers.Collection.MetaCollectionIndex,
					BarbadosIdentifiers.Collection.MetaCollection,
					BarbadosIdentifiers.MetaCollection.CollectionDocumentNameFieldAbsolute,
					this,
					new()
					{
						KeyMaxLength = Constants.MetaCollectionIndexKeyMaxLength,
						RootPageHandle = root.MappingCollectionNameIndexRootPageHandle
					}
				);

				var clusteredIndex = new BTreeClusteredIndex(this, root.MetaCollectionClusteredIndexRootPageHandle);
				var meta = new MetaCollection(this, root.MetaCollectionPageHandle, index, clusteredIndex);

				Pool.Release(root);

				var r = _instances.TryAdd(BarbadosIdentifiers.Collection.MetaCollection, meta);
				Debug.Assert(r);

				Lock.AddLockable(BarbadosIdentifiers.Collection.MetaCollection);
				Lock.AddLockable(BarbadosIdentifiers.Collection.MetaCollectionIndex);
				return meta;
			}
		}
	}
}
