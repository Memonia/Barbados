using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine
{
	internal sealed class Cursor<T> : ICursor<T>
	{
		public BarbadosIdentifier OwnerName { get; }

		private int _open;
		private int _closed;
		private readonly IEnumerable<T> _enumerable;
		private readonly BarbadosController _controller;

		public Cursor(IEnumerable<T> enumerable, BarbadosController controller, BarbadosIdentifier ownerName)
		{
			OwnerName = ownerName;
			_open = 0;
			_closed = 0;
			_enumerable = enumerable;
			_controller = controller;
		}

		public void Close()
		{
			if (Interlocked.CompareExchange(ref _closed, 1, 0) == 0)
			{
				_controller.Lock.Release(OwnerName, LockMode.Read);
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			if (Interlocked.CompareExchange(ref _open, 1, 0) != 0)
			{
				throw new BarbadosException(BarbadosExceptionCode.CursorConsumed, "Cursor has been opened already");
			}

			_controller.Lock.Acquire(OwnerName, LockMode.Read);
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

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
