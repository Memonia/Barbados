using System.Collections.Generic;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Collections
{
	internal abstract partial class AbstractCollection : IBarbadosCollection
	{
		public BarbadosIdentifier Name { get; set; }
		public PageHandle CollectionPageHandle { get; }
		public PagePool Pool { get; }
		public LockAutomatic Lock { get; }
		public BTreeClusteredIndex ClusteredIndex { get; }

		protected List<BTreeIndex> Indexes { get; }

		private readonly object _sync;

		protected AbstractCollection(
			BarbadosIdentifier name,
			PageHandle collectionPageHandle,
			PagePool pool,
			LockAutomatic @lock,
			BTreeClusteredIndex clusteredIndex
		)
		{
			Name = name;
			CollectionPageHandle = collectionPageHandle;
			Pool = pool;
			Lock = @lock;
			ClusteredIndex = clusteredIndex;

			_sync = new();
			Indexes = [];
		}

		public void DeallocateNoLock()
		{
			foreach (var index in Indexes)
			{
				index.DeallocateNoLock();
			}

			ClusteredIndex.DeallocateNoLock();
		}

		public void AddBTreeIndex(BTreeIndex index)
		{
			if (TryGetBTreeIndex(index.IndexedField, out _))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.IndexAlreadyExists,
					$"Index on field {index.IndexedField} has been added to the current instance already"
				);
			}

			lock (_sync)
			{
				Indexes.Add(index);
			}
		}

		public void RemoveBTreeIndex(BarbadosIdentifier field)
		{
			lock (_sync)
			{
				for (int i = 0; i < Indexes.Count; i++)
				{
					if (Indexes[i].IndexedField.Identifier == field.Identifier)
					{
						Indexes.RemoveAt(i);
						return;
					}
				}

				throw new BarbadosException(
					BarbadosExceptionCode.IndexDoesNotExist,
					$"Index on field {field} does not exist in the current instance"
				);
			}
		}

		public bool TryGetBTreeIndex(BarbadosIdentifier field, out BTreeIndex index)
		{
			lock (_sync)
			{
				foreach (var storedIndex in Indexes)
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

		public bool TryGetBTreeIndexLookup(BarbadosIdentifier field, out IBTreeIndexLookup lookup)
		{
			if (TryGetBTreeIndex(field, out var index))
			{
				lookup = index;
				return true;
			}

			lookup = default!;
			return false;
		}

		public ObjectId Insert(BarbadosDocument document)
		{
			using (Lock.Acquire(LockMode.Write))
			{
				return InsertNoLock(document);
			}
		}

		public bool TryRead(ObjectId id, out BarbadosDocument document)
		{
			return TryRead(id, ValueSelector.SelectAll, out document);
		}

		public bool TryRead(ObjectId id, ValueSelector selector, out BarbadosDocument document)
		{
			using (Lock.Acquire(LockMode.Read))
			{
				return TryReadNoLock(id, selector, out document);
			}
		}

		public bool TryUpdate(ObjectId id, BarbadosDocument document)
		{
			using (Lock.Acquire(LockMode.Write))
			{
				return TryUpdateNoLock(id, document);
			}
		}

		public bool TryRemove(ObjectId id)
		{
			using (Lock.Acquire(LockMode.Write))
			{
				return TryRemoveNoLock(id);
			}
		}

		public void Update(ObjectId id, BarbadosDocument document)
		{
			if (!TryUpdate(id, document))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DocumentNotFound, $"Document with id {id} not found"
				);
			}
		}

		public void Remove(ObjectId id)
		{
			if (!TryRemove(id))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DocumentNotFound, $"Document with id {id} not found"
				);
			}
		}

		public void Read(ObjectId id, out BarbadosDocument document)
		{
			Read(id, ValueSelector.SelectAll, out document);
		}

		public void Read(ObjectId id, ValueSelector selector, out BarbadosDocument document)
		{
			if (!TryRead(id, selector, out document))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DocumentNotFound, $"Document with id {id} not found"
				);
			}
		}

		protected ObjectId InsertNoLock(BarbadosDocument document)
		{
			var collection = Pool.LoadPin<CollectionPage>(CollectionPageHandle);
			if (collection.TryGetNextObjectId(out var nextId))
			{
				// Save next available document id
				Pool.SaveRelease(collection);

				InsertNoLock(nextId, document);
				return nextId;
			}

			else
			{
				Pool.Release(collection);
				throw new BarbadosException(BarbadosExceptionCode.MaxDocumentCountReached);
			}
		}

		protected void InsertNoLock(ObjectId id, BarbadosDocument document)
		{
			var idn = new ObjectIdNormalised(id);
			ClusteredIndex.Insert(idn, document.Buffer, CollectionPageHandle);

			foreach (var index in Indexes)
			{
				if (document.Buffer.TryGetNormalisedValue(index.IndexedField.StringBufferValue, out var value))
				{
					index.Insert(value, id);
				}
			}
		}

		protected bool TryReadNoLock(ObjectId id, ValueSelector selector, out BarbadosDocument document)
		{
			var idn = new ObjectIdNormalised(id);
			var result = false;
			if (ClusteredIndex.TryRead(idn, out var handle))
			{
				result = ObjectReader.TryRead(Pool, handle, id, selector, out var obj);
				if (!result)
				{
					throw new BarbadosException(BarbadosExceptionCode.InternalError);
				}

				document = new(id, obj);
			}

			else
			{
				document = default!;
			}

			return result;
		}

		protected bool TryUpdateNoLock(ObjectId id, BarbadosDocument document)
		{
			bool result = false;
			if (TryRemoveNoLock(id))
			{
				result = true;
				InsertNoLock(id, document);
			}

			return result;
		}

		protected bool TryRemoveNoLock(ObjectId id)
		{
			var idn = new ObjectIdNormalised(id);
			if (!TryReadNoLock(id, ValueSelector.SelectAll, out var document))
			{
				return false;
			}

			if (!ClusteredIndex.TryRemove(idn, CollectionPageHandle))
			{
				throw new BarbadosException(BarbadosExceptionCode.InternalError);
			}

			foreach (var index in Indexes)
			{
				if (document.Buffer.TryGetNormalisedValue(index.IndexedField.StringBufferValue, out var value))
				{
					if (!index.TryRemove(value, id))
					{
						throw new BarbadosException(BarbadosExceptionCode.InternalError);
					}
				}
			}

			return true;
		}
	}
}
