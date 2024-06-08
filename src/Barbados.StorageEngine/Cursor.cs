using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal sealed class Cursor<T> : ICursor<T>, IDisposable
	{
		/*	Since the lock is released between yields, cursor providers  
		 *	have to ensure that writing in-between the reads will not break anything
		 */

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
			if (Interlocked.CompareExchange(ref _open, 1, 0) != 0)
			{
				Close();
				throw new BarbadosException(
					BarbadosExceptionCode.CursorConsumed, $"Current cursor is already consumed"
				);
			}

			_lock.Acquire(LockMode.Read);
			var e = _enumerable.GetEnumerator();
			try
			{
				while (e.MoveNext())
				{
					_lock.Release(LockMode.Read);

					yield return e.Current; 
					if (Interlocked.CompareExchange(ref _closed, _closed, 1) == 1)
					{
						throw new BarbadosException(
							BarbadosExceptionCode.CursorClosed, $"Current cursor has been closed"
						);
					}

					_lock.Acquire(LockMode.Read);
				}
			}

			finally
			{
				Close();
			}
		}

		public void Dispose()
		{
			Close();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
