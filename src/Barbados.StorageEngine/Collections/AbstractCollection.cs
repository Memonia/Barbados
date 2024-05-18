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
			=> LockableInsert(document, toLock: true);

		public bool TryRead(ObjectId id, out BarbadosDocument document) =>
			TryRead(id, ValueSelector.SelectAll, out document);

		public bool TryRead(ObjectId id, ValueSelector selector, out BarbadosDocument document) =>
			TryLockableRead(id, selector, toLock: true, out document);

		public bool TryUpdate(ObjectId id, BarbadosDocument document) =>
			TryLockableUpdate(id, document, toLock: true);

		public bool TryRemove(ObjectId id) =>
			TryLockableRemove(id, toLock: true);

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

		protected ObjectId LockableInsert(BarbadosDocument document, bool toLock)
		{
			if (toLock)
			{
				Controller.Lock.Acquire(Name, LockMode.Write);
			}

			var collection = Controller.Pool.LoadPin<CollectionPage>(CollectionPageHandle);
			if (collection.TryGetNextObjectId(out var nextId))
			{
				// Save next available document id
				Controller.Pool.SaveRelease(collection);

				_insert(nextId, document, toLock: false);
				if (toLock)
				{
					Controller.Lock.Release(Name, LockMode.Write);
				}

				return nextId;
			}

			else
			{
				Controller.Pool.Release(collection);
				if (toLock)
				{
					Controller.Lock.Release(Name, LockMode.Write);
				}

				throw new BarbadosException(BarbadosExceptionCode.MaxDocumentCountReached);
			}
		}

		protected bool TryLockableRead(ObjectId id, ValueSelector selector, bool toLock, out BarbadosDocument document)
		{
			var idn = new ObjectIdNormalised(id);
			if (toLock)
			{
				Controller.Lock.Acquire(Name, LockMode.Read);
			}

			var result = false;
			document = default!;
			if (ClusteredIndex.TryRead(idn, out var handle))
			{
				result = ObjectReader.Read(Controller.Pool, new(id, handle), selector, out var obj);
				if (!result)
				{
					throw new BarbadosException(BarbadosExceptionCode.InternalError);
				}

				document = new(id, obj);
			}

			if (toLock)
			{
				Controller.Lock.Release(Name, LockMode.Read);
			}

			return result;
		}

		protected bool TryLockableUpdate(ObjectId id, BarbadosDocument document, bool toLock)
		{
			if (toLock)
			{
				Controller.Lock.Acquire(Name, LockMode.Write);
			}

			bool result = false;
			if (_tryRemove(id, toLock: false))
			{
				result = true;
				_insert(id, document, toLock: false);
			}

			if (toLock)
			{
				Controller.Lock.Release(Name, LockMode.Write);
			}

			return result;
		}

		protected bool TryLockableRemove(ObjectId id, bool toLock)
		{
			return _tryRemove(id, toLock);
		}

		private void _insert(ObjectId id, BarbadosDocument document, bool toLock)
		{
			var idn = new ObjectIdNormalised(id);
			if (toLock)
			{
				Controller.Lock.Acquire(Name, LockMode.Write);
			}

			ClusteredIndex.Insert(idn, document.Buffer, CollectionPageHandle);
			foreach (var index in Indexes)
			{
				if (document.Buffer.TryGetNormalisedValue(index.IndexedField.StringBufferValue, out var value))
				{
					index.Insert(value, id);
				}
			}

			if (toLock)
			{
				Controller.Lock.Release(Name, LockMode.Write);
			}
		}

		private bool _tryRemove(ObjectId id, bool toLock)
		{
			var idn = new ObjectIdNormalised(id);
			if (toLock)
			{
				Controller.Lock.Acquire(Name, LockMode.Write);
			}

			if (!TryLockableRead(id, ValueSelector.SelectAll, toLock: false, out var document))
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

			if (toLock)
			{
				Controller.Lock.Release(Name, LockMode.Write);
			}

			return true;
		}
	}
}
