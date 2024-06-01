using System;
using System.Diagnostics;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Collections.Internal;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine
{
	internal partial class BarbadosController
	{
		private static void _buildIndex(AbstractCollection collection, BTreeIndex index)
		{
			var cursor = collection.GetCursor();
			foreach (var document in cursor)
			{
				if (document.Buffer.TryGetNormalisedValue(index.Field.StringBufferValue, out var value))
				{
					index.Insert(value, document.Id);
				}
			}
		}

		public void CreateIndex(BarbadosIdentifier collection, BarbadosIdentifier field)
		{
			_createIndex(collection, field, 0, useDefaultLength: true);
		}

		public void CreateIndex(BarbadosIdentifier collection, BarbadosIdentifier field, int maxKeyLength)
		{
			if (maxKeyLength < 1)
			{
				throw new ArgumentException("Expected a positive integer above zero", nameof(maxKeyLength));
			}

			_createIndex(collection, field, maxKeyLength, useDefaultLength: false);
		}

		public void RemoveIndex(BarbadosIdentifier collection, BarbadosIdentifier field)
		{
			if (collection.IsReserved)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.InvalidOperation,
					$"Attempted to remove an index on an internal collection '{collection}'"
				);
			}

			lock (_sync)
			{
				if (!TryGetIndex(collection, field, out var index))
				{
					throw new BarbadosException(
						BarbadosExceptionCode.IndexDoesNotExist,
						$"Index on field '{field}' in collection '{collection}' does not exist"
					);
				}

				Debug.Assert(index.Field.Identifier == field.Identifier);

				var meta = GetMetaCollection();
				var collectionInstance = GetCollection(collection);
				var r = meta.Find(collection, out var document);
				Debug.Assert(r);

				Lock.RemoveLockable(index.Name, out var @lock);
				@lock.EnterWriteLock();

				Lock.Acquire(collectionInstance.Name, LockMode.Write);

				meta.RemoveIndex(document, field);
				collectionInstance.RemoveBTreeIndex(index.Field);
				
				Lock.Release(collectionInstance.Name, LockMode.Write);

				index.DeallocateNoLock();
				@lock.ExitWriteLock();
			}
		}

		public void CreateCollection(BarbadosIdentifier name)
		{
			if (name.IsReserved)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.InvalidOperation, $"'?<name>' format is reserved for internal use"
				);
			}

			var meta = GetMetaCollection();
			if (meta.Find(name, out _))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.CollectionAlreadyExists, $"Collection '{name}' already exists"
				);
			}

			meta.Create(name);
		}

		public void RemoveCollection(BarbadosIdentifier name)
		{
			lock (_sync)
			{
				var meta = GetMetaCollection();
				var collection = GetCollection(name);
				var r = meta.Find(name, out var document);
				Debug.Assert(r);

				// Prevent start of any new operations
				Lock.RemoveLockable(name, out var @lock);

				// Wait for pending operations to finish
				@lock.EnterWriteLock();

				r = _instances.TryRemove(name, out _);
				Debug.Assert(r);

				meta.Remove(document);
				collection.DeallocateNoLock();

				@lock.ExitWriteLock();
			}
		}

		public void RenameCollection(BarbadosIdentifier name, BarbadosIdentifier replacement)
		{
			if (replacement.IsReserved)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.InvalidOperation, $"'?<name>' format is reserved for internal use"
				);
			}

			lock (_sync)
			{
				var meta = GetMetaCollection();
				if (meta.Find(replacement, out _))
				{
					throw new BarbadosException(
						BarbadosExceptionCode.CollectionAlreadyExists, $"Collection with name '{replacement}' already exists"
					);
				}

				var collection = GetCollection(name);
				var r = !meta.Find(name, out var document);

				Lock.Acquire(name, LockMode.Write);

				meta.Rename(document, replacement);

				collection.Name = replacement;

				r = _instances.TryRemove(name, out _);
				Debug.Assert(r);

				r = _instances.TryAdd(replacement, collection);
				Debug.Assert(r);

				Lock.RemoveLockable(name, out var @lock);
				Lock.AddLockable(replacement);
				@lock.ExitWriteLock();
			}
		}

		private void _createIndex(BarbadosIdentifier collection, BarbadosIdentifier field, int maxKeyLength, bool useDefaultLength)
		{
			if (collection.IsReserved)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.InvalidOperation,
					$"Attempted to create an index on an internal collection '{collection}'"
				);
			}

			if (field.IsGroup)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.InvalidOperation,
					$"Cannot create an index on the whole group '{field}'. Only fields are supported"
				);
			}

			lock (_sync)
			{
				if (useDefaultLength)
				{
					maxKeyLength = -1;
				}

				var meta = GetMetaCollection();
				if (!meta.Find(collection, out var document))
				{
					throw new BarbadosException(
						BarbadosExceptionCode.CollectionDoesNotExist, $"Collection '{collection}' does not exist"
					);
				}

				var collectionLoaded = _instances.TryGetValue(collection, out var collectionInstance);
				if (!collectionLoaded)
				{
					collectionInstance = GetCollection(collection);
				}

				var idoc = meta.CreateIndex(document, field, maxKeyLength);
				var iname = MetaCollection.GetIndexName(idoc, collection);
				Lock.AddLockable(iname);

				var ilock = Lock.GetLock(iname);
				var iinstance = MetaCollection.CreateIndexInstance(
					idoc, iname, Pool, ilock, collectionInstance!.ClusteredIndex
				);

				collectionInstance!.AddBTreeIndex(iinstance);
				_buildIndex(collectionInstance!, iinstance);
			}
		}
	}
}
