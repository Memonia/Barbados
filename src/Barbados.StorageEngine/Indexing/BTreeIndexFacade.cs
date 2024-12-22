using System.Threading;

using Barbados.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed partial class BTreeIndexFacade : AbstractBTreeIndexFacade, IReadOnlyBTreeIndex
	{
		public ObjectId CollectionId => Info.CollectionId;
		public BarbadosKey IndexField => Info.IndexField;

		public BarbadosKeySelector KeySelector { get; }
		public bool IsDeleted
		{
			get
			{
				Interlocked.MemoryBarrier();
				return _isDeleted;
			}

			set
			{
				_isDeleted = value;
				Interlocked.MemoryBarrier();
			}
		}

		private readonly TransactionManager _txManager;
		private readonly BTreeClusteredIndexFacade _clusteredIndexFacade;

		private bool _isDeleted;

		public BTreeIndexFacade(
			TransactionManager transactionManager,
			BTreeClusteredIndexFacade clusteredIndexFacade,
			BTreeIndexInfo info
		) : base(info)
		{
			KeySelector = new(Info.IndexField);
			IsDeleted = false;
			_txManager = transactionManager;
			_clusteredIndexFacade = clusteredIndexFacade;
		}

		public BTreeIndexTransactionProxy GetProxy(TransactionScope transaction)
		{
			_ensureExists();
			return new(transaction, Info);
		}

		public ICursor<ObjectId> FindExact<T>(T searchValue)
		{
			_ensureExists();
			return new BTreeIndexFindCursor(
				CollectionId, _txManager, this, _clusteredIndexFacade,
				BTreeIndexFindCursorParameters.GetFindExactParameters(searchValue)
			);
		}

		public ICursor<ObjectId> Find(BarbadosDocument condition)
		{
			_ensureExists();
			return new BTreeIndexFindCursor(
				CollectionId, _txManager, this, _clusteredIndexFacade,
				BTreeIndexFindCursorParameters.GetFindParameters(condition)
			);
		}

		private void _ensureExists()
		{
			if (IsDeleted)
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.IndexDoesNotExist,
					$"Index for field '{IndexField}' in collection with id {CollectionId} does not exist"
				);
			}
		}
	}
}
