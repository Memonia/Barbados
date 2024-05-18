using System;
using System.Collections.Concurrent;
using System.Threading;

using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal sealed partial class LockManager
	{
		private static void _acquire(ReaderWriterLockSlim @lock, LockMode mode)
		{
			switch (mode)
			{
				case LockMode.Read:
					@lock.EnterReadLock();
					break;

				case LockMode.Write:
					@lock.EnterWriteLock();
					break;

				default:
					throw new NotImplementedException();
			}
		}

		private static void _release(ReaderWriterLockSlim @lock, LockMode mode)
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

		private readonly ConcurrentDictionary<string, ReaderWriterLockSlim> _lockables;

		public LockManager()
		{
			_lockables = new();
		}

		public void AddLockable(string name)
		{
			if (!_lockables.TryAdd(name, new()))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.LockableDoesNotExist, $"Lockable '{name}' already exists"
				);
			}
		}

		public void RemoveLockable(string name, out ReaderWriterLockSlim @lock)
		{
			if (!_lockables.TryRemove(name, out @lock!))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.LockableDoesNotExist, $"Lockable '{name}' does not exist"
				);
			}
		}

		public bool ContainsLockable(string name)
		{
			return _lockables.ContainsKey(name);
		}

		public void Acquire(string name, LockMode mode)
		{
			if (!_lockables.TryGetValue(name, out var @lock))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.LockableDoesNotExist, $"Lockable '{name}' does not exist"
				);
			}

			_acquire(@lock, mode);
			if (!ContainsLockable(name))
			{
				_release(@lock, mode);
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.StaleLockable, $"Lockable '{name} no longer exists'"
				);
			}
		}

		public void Release(string name, LockMode mode)
		{
			if (!_lockables.TryGetValue(name, out var @lock))
			{
				throw new BarbadosConcurrencyException(
					BarbadosExceptionCode.LockableDoesNotExist, $"Lockable '{name}' does not exist"
				);
			}

			_release(@lock, mode);
		}
	}
}
