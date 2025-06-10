using System;
using System.Collections.Concurrent;
using System.Diagnostics;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage.Wal;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine.Transactions
{
	internal sealed partial class TransactionManager
	{
		private static readonly BarbadosException _transactionScopeMismatch = new(BarbadosExceptionCode.TransactionScopeMismatch,
			"Failed to complete the transaction. A more recent transaction scope is still active. " +
			"Make sure cursors and other disposable database objects have been disposed of"
		);

		private static readonly BarbadosException _transactionScopeCompleted = new(BarbadosExceptionCode.TransactionScopeCompleted,
			"Current transaction scope has already been completed"
		);

		private static TransactionScope _popCurrentScope(Transaction transaction)
		{
			if (!transaction.TransactionScopes.TryPop(out var scope))
			{
				throw new BarbadosInternalErrorException();
			}

			return scope;
		}

		private static void _completeTransaction(Transaction tx)
		{
			foreach (var scope in tx.LockScopes)
			{
				scope.Dispose();
			}
		}

		private readonly System.Threading.Lock _sync;
		private readonly TimeSpan _defaultTimeout;
		private readonly WalBuffer _wal;
		private readonly LockManager _lockManager;
		private readonly ConcurrentDictionary<int, Transaction> _transactions;

		private ObjectId _nextTransactionId;

		public TransactionManager(TimeSpan defaultAcquireTimeout, WalBuffer wal, LockManager lockManager)
		{
			_sync = new();
			_defaultTimeout = defaultAcquireTimeout;
			_wal = wal;
			_lockManager = lockManager;
			_transactions = [];
			_nextTransactionId = new(ObjectId.Invalid.Value + 1);
		}

		public TransactionScope GetAutomaticTransaction(ObjectId lockId, TransactionMode mode)
		{
			if (_transactions.TryGetValue(Environment.CurrentManagedThreadId, out var tx))
			{
				if (tx.Mode == TransactionMode.Read && mode == TransactionMode.ReadWrite)
				{
					throw new BarbadosException(
						BarbadosExceptionCode.TransactionUpgradeAttempt, "Cannot upgrade read-only transaction"
					);
				}

				var lockTaken = false;
				foreach (var takenLock in tx.LockScopes)
				{
					if (takenLock.Lock.Id.Value == lockId.Value)
					{
						lockTaken = true;
						break;
					}
				}

				if (!lockTaken)
				{
					throw new BarbadosException(BarbadosExceptionCode.TransactionTargetMismatch,
						$"Object with id {lockId} is not a part of the current transaction"
					);
				}

				var scope = new TransactionScope(tx.Snapshot, mode, _wal);
				tx.TransactionScopes.Push(scope);

				scope.OnCompleted += _onCompletedTransactionScope;
				return scope;
			}

			var @lock = _lockManager.GetLock(lockId);
			var lmode = mode switch
			{
				TransactionMode.Read => LockMode.Read,
				TransactionMode.ReadWrite => LockMode.Write,
				_ => throw new NotImplementedException()
			};

			var lscope = @lock.Acquire(lmode);
			var txid = _incrementNextTransactionId();
			var snapshot = _wal.TakeSnapshot(txid);
			tx = new Transaction(txid, snapshot, mode, [lscope]);
			var tscope = new TransactionScope(tx.Snapshot, mode, _wal);

			tx.TransactionScopes.Push(tscope);
			if (!_transactions.TryAdd(Environment.CurrentManagedThreadId, tx))
			{
				throw new BarbadosInternalErrorException();
			}

			tscope.OnCompleted += _onCompletedTransactionScope;
			return tscope;
		}

		public TransactionBuilder CreateTransaction(TransactionMode mode)
		{
			if (_transactions.TryGetValue(Environment.CurrentManagedThreadId, out _))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.NestedTransactionDetected, "Nested transactions are not supported"
				);
			}

			var txid = _incrementNextTransactionId();
			var txbuilder = new TransactionBuilder(txid, mode, _defaultTimeout, _wal, _lockManager);
			txbuilder.OnCreated += _onCreatedTransaction;
			return txbuilder;
		}

		public void CommitCurrentTransaction()
		{
			var tx = _getCurrentTransaction();
			var scope = _popCurrentScope(tx);
			_commit(tx, scope);
		}

		public void RollbackCurrentTransaction()
		{
			var tx = _getCurrentTransaction();
			var scope = _popCurrentScope(tx);
			_rollback(tx, scope);
		}

		public void CommitTransaction(TransactionScope scope)
		{
			if (!scope.IsActive)
			{
				throw _transactionScopeCompleted;
			}

			var tx = _getCurrentTransaction();
			if (_popCurrentScope(tx) != scope)
			{
				throw _transactionScopeMismatch;
			}

			_commit(tx, scope);
		}

		public void RollbackTransaction(TransactionScope scope)
		{
			if (!scope.IsActive)
			{
				throw _transactionScopeCompleted;
			}

			var tx = _getCurrentTransaction();
			if (_popCurrentScope(tx) != scope)
			{
				throw _transactionScopeMismatch;
			}

			_rollback(tx, scope);
		}

		private Transaction _getCurrentTransaction()
		{
			if (!_transactions.TryGetValue(Environment.CurrentManagedThreadId, out var tx))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.TransactionDoesNotExist, "Current thread has no active transactions"
				);
			}

			return tx;
		}

		private void _commit(Transaction transaction, TransactionScope scope)
		{
			scope.IsActive = false;
			if (transaction.TransactionScopes.Count == 0)
			{
				if (!_transactions.TryRemove(Environment.CurrentManagedThreadId, out _))
				{
					throw BarbadosInternalErrorExceptionHelpers.TransactionForCurrentThreadDoesNotExist();
				}

				_wal.Commit(scope.Snapshot);
				_completeTransaction(transaction);
			}
		}

		private void _rollback(Transaction transaction, TransactionScope scope)
		{
			scope.IsActive = false;
			if (transaction.TransactionScopes.Count == 0)
			{
				if (!_transactions.TryRemove(Environment.CurrentManagedThreadId, out _))
				{
					throw BarbadosInternalErrorExceptionHelpers.TransactionForCurrentThreadDoesNotExist();
				}

				_wal.Rollback(scope.Snapshot);
				_completeTransaction(transaction);
			}
		}

		private void _onCreatedTransaction(Transaction transaction)
		{
			Debug.Assert(transaction.TransactionScopes.Count == 1);
			if (!_transactions.TryAdd(Environment.CurrentManagedThreadId, transaction))
			{
				throw new BarbadosInternalErrorException("Current thread already has an open transaction associated with it");
			}

			transaction.TransactionScopes.Peek().OnCompleted += _onCompletedTransactionScope;
		}

		private void _onCompletedTransactionScope(TransactionScope scope)
		{
			// Transaction was commited or rolled-back, no need to do anything else
			if (!scope.IsActive)
			{
				return;
			}

			RollbackTransaction(scope);
		}

		private ObjectId _incrementNextTransactionId()
		{
			lock (_sync)
			{
				if (_nextTransactionId.Value == ObjectId.Invalid.Value)
				{
					throw new BarbadosException(BarbadosExceptionCode.MaxTransactionCountReached);
				}

				var id = _nextTransactionId;
				_nextTransactionId = new(id.Value + 1);
				return id;
			}
		}
	}
}
