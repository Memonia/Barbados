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
		private readonly ObjectLock _lock;
		private readonly IEnumerable<T> _enumerable;

		public Cursor(IEnumerable<T> enumerable, ObjectLock @lock)
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
				_lock.Dispose();
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (Interlocked.CompareExchange(ref _open, 1, 0) != 0)
			{
				throw new BarbadosException(BarbadosExceptionCode.CursorConsumed, "Cursor has been opened already");
			}

			_lock.Acquire();
			foreach (var e in _enumerable)
			{
				yield return e;

				if (Interlocked.CompareExchange(ref _closed, _closed, 1) == 1)
				{
					throw new BarbadosException(BarbadosExceptionCode.CursorClosed, "Cursor has been closed");
				}
			}

			Close();
		}

		public void Dispose()
		{
			Close();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
