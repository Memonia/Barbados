using System;
using System.Collections.Concurrent;
using System.Threading;

using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine.Transactions.Locks
{
	internal sealed partial class LockManager
	{
		private readonly ConcurrentDictionary<ObjectId, ReaderWriterLockSlim> _locks;

		public LockManager()
		{
			_locks = [];
		}

		public bool ContainsLock(ObjectId id)
		{
			return _locks.ContainsKey(id);
		}

		public bool TryGetLock(ObjectId id, out Lock @lock)
		{
			if (_locks.ContainsKey(id))
			{
				@lock = new(id, this);
				return true;
			}

			@lock = default!;
			return false;
		}

		public Lock GetLock(ObjectId id)
		{
			if (!TryGetLock(id, out var @lock))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.LockDoesNotExist, $"Lock for '{id}' does not exist"
				);
			}

			return @lock;
		}

		public void CreateLock(ObjectId id)
		{
			if (!_locks.TryAdd(id, new()))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.LockDoesNotExist, $"Lock for '{id}' already exists"
				);
			}
		}

		public void RemoveLock(ObjectId id, out ReaderWriterLockSlim @lock)
		{
			if (!_locks.TryRemove(id, out @lock!))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.LockDoesNotExist, $"Lock for '{id}' does not exist"
				);
			}
		}

		public void Acquire(ObjectId id, LockMode mode)
		{
			TryAcquire(id, mode, Timeout.InfiniteTimeSpan);
		}

		public bool TryAcquire(ObjectId id, LockMode mode, TimeSpan timeout)
		{
			if (!_locks.TryGetValue(id, out var @lock))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.LockDoesNotExist, $"Lock for '{id}' does not exist"
				);
			}

			return mode switch
			{
				LockMode.Read => @lock.TryEnterReadLock(timeout),
				LockMode.Write => @lock.TryEnterWriteLock(timeout),
				_ => throw new NotImplementedException(),
			};
		}

		public void Release(ObjectId id, LockMode mode)
		{
			if (_locks.TryGetValue(id, out var @lock))
			{
				switch (mode)
				{
					case LockMode.Read:
						@lock.ExitReadLock();
						break;

					case LockMode.Write:
						@lock.ExitWriteLock();
						break;

					default:
						throw new NotImplementedException();
				}
			}
		}

		public bool IsLocked(ObjectId id, LockMode mode)
		{
			if (_locks.TryGetValue(id, out var @lock))
			{
				return mode switch
				{
					LockMode.Read => @lock.IsReadLockHeld,
					LockMode.Write => @lock.IsWriteLockHeld,
					_ => throw new NotImplementedException(),
				};
			}

			return false;
		}
	}
}