using System;
using System.Threading;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal abstract class BaseCursor : IDisposable
	{
		protected Lazy<TransactionScope> Transaction { get; set; }

		private int _open;
		private int _closed;

		public BaseCursor(Lazy<TransactionScope> transaction)
		{
			Transaction = transaction;
			_open = 0;
			_closed = 0;
		}

		public void Close()
		{
			if (Interlocked.CompareExchange(ref _closed, 1, 0) == 0)
			{
				if (Transaction.IsValueCreated)
				{
					Transaction.Value.Dispose();
				}
			}
		}

		public void Dispose()
		{
			Close();
		}

		protected void EnsureNotOpen()
		{
			if (Interlocked.CompareExchange(ref _open, 1, 0) != 0)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.CursorConsumed, $"Current cursor is already consumed"
				);
			}
		}

		protected void EnsureNotClosed()
		{
			if (Interlocked.CompareExchange(ref _closed, _closed, 1) == 1)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.CursorClosed, $"Current cursor has been closed"
				);
			}
		}
	}
}
