using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal sealed class Cursor<T> : ICursor<T>, IDisposable
	{
		public BarbadosIdentifier OwnerName => _lock.Name;

		private int _open;
		private int _closed;
		private readonly LockAutomatic _lock;
		private readonly IEnumerable<T> _enumerable;

		public Cursor(IEnumerable<T> enumerable, LockAutomatic @lock)
		{
			_open = 0;
			_closed = 0;
			_lock = @lock;
			_enumerable = enumerable;
		}

		public void Close()
		{
			if (Interlocked.CompareExchange(ref _closed, 1, 0) == 0)
			{
				_lock.Release(LockMode.Read);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			_lock.Acquire(LockMode.Read);
			if (Interlocked.CompareExchange(ref _open, 1, 0) != 0)
			{
				_lock.Release(LockMode.Read);
				throw new BarbadosException(
					BarbadosExceptionCode.CursorConsumed, $"Current cursor on {OwnerName} is already open"
				);
			}

			foreach (var e in _enumerable)
			{
				yield return e;

				if (Interlocked.CompareExchange(ref _closed, _closed, 1) == 1)
				{
					throw new BarbadosException(
						BarbadosExceptionCode.CursorClosed, $"Current cursor on {OwnerName} has been closed"
					);
				}
			}
		}

		public void Dispose()
		{
			Close();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
