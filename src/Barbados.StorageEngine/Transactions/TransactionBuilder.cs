using System;
using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Storage.Wal;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine.Transactions
{
	internal sealed partial class TransactionBuilder : ITransactionBuilder
	{
		public event Action<Transaction>? OnCreated;

		private readonly ObjectId _id;
		private readonly TransactionMode _mode;
		private readonly TimeSpan _defaultTimeout;
		private readonly WalBuffer _wal;
		private readonly LockManager _lockManager;
		private readonly Dictionary<ObjectId, (Lock, LockMode)> _locks;

		private bool _built;

		public TransactionBuilder(
			ObjectId id,
			TransactionMode mode,
			TimeSpan timeout,
			WalBuffer wal,
			LockManager lockManager
		)
		{
			_id = id;
			_mode = mode;
			_defaultTimeout = timeout;
			_wal = wal;
			_locks = [];
			_lockManager = lockManager;
			_built = false;
		}

		public TransactionScope BeginTransaction()
		{
			return BeginTransaction(_defaultTimeout);
		}

		public TransactionScope BeginTransaction(TimeSpan timeout)
		{
			if (_built)
			{
				throw new InvalidOperationException("Cannot reuse transaction builder");
			}

			_built = true;

			var scopes = new List<Lock.AcquireScope>(_locks.Count);
			foreach (var (@lock, mode) in _locks.Values)
			{
				try
				{
					if (!@lock.TryAcquire(mode, timeout, out var scope))
					{
						throw new TimeoutException($"Failed to acquire lock on object with id {@lock.Id}");
					}

					scopes.Add(scope);
				}

				catch
				{
					foreach (var scope in scopes)
					{
						scope.Dispose();
					}

					throw;
				}
			}

			var snapshot = _wal.TakeSnapshot(_id);
			var tx = new Transaction(_id, snapshot, _mode, scopes);
			var tscope = new TransactionScope(snapshot, _mode, _wal);

			tx.TransactionScopes.Push(tscope);
			OnCreated?.Invoke(tx);
			return tscope;
		}

		public TransactionBuilder IncludeLock(ObjectId id, LockMode mode)
		{
			Debug.Assert(
				_mode == TransactionMode.Read || (
				_mode == TransactionMode.ReadWrite && mode == LockMode.Write
				)
			);

			var @lock = _lockManager.GetLock(id);
			if (!_locks.TryAdd(id, (@lock, mode)))
			{
				throw new InvalidOperationException($"Object with id {id} already included");
			}

			return this;
		}
	}
}
