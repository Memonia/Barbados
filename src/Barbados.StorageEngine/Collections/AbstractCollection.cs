using System.Collections.Generic;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Collections
{
	internal abstract partial class AbstractCollection : IBarbadosCollection
	{
		public BarbadosIdentifier Name { get; set; }
		public BarbadosController Controller { get; }
		public PageHandle CollectionPageHandle { get; }
		public BTreeClusteredIndex ClusteredIndex { get; }

		public List<BTreeIndex> Indexes { get; }

		IBarbadosController IBarbadosReadOnlyCollection.Controller => Controller;

		protected AbstractCollection(
			BarbadosIdentifier name,
			BarbadosController controller,
			PageHandle collectionPageHandle,
			List<BTreeIndex> indexes,
			BTreeClusteredIndex clusteredIndex
		)
		{
			Name = name;
			Controller = controller;
			Indexes = indexes;
			CollectionPageHandle = collectionPageHandle;
			ClusteredIndex = clusteredIndex;
		}

		public void DeallocateNoLock()
		{
			foreach (var index in Indexes)
			{
				index.DeallocateNoLock();
			}

			ClusteredIndex.DeallocateNoLock();
		}

		public ObjectId Insert(BarbadosDocument document)
		{
			using (Controller.GetLock(Name).Acquire(LockMode.Write))
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
			using (Controller.GetLock(Name).Acquire(LockMode.Read))
			{
				return TryReadNoLock(id, selector, out document);
			}
		}

		public bool TryUpdate(ObjectId id, BarbadosDocument document)
		{
			using (Controller.GetLock(Name).Acquire(LockMode.Write))
			{
				return TryUpdateNoLock(id, document);
			}
		}

		public bool TryRemove(ObjectId id)
		{
			using (Controller.GetLock(Name).Acquire(LockMode.Write))
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
			var collection = Controller.Pool.LoadPin<CollectionPage>(CollectionPageHandle);
			if (collection.TryGetNextObjectId(out var nextId))
			{
				// Save next available document id
				Controller.Pool.SaveRelease(collection);

				InsertNoLock(nextId, document);
				return nextId;
			}

			else
			{
				Controller.Pool.Release(collection);
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
				result = ObjectReader.Read(Controller.Pool, handle, id, selector, out var obj);
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
