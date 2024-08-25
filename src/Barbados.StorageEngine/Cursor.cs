using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine
{
	internal abstract class Cursor<T> : ICursor<T>, IDisposable
	{
		public ObjectId CollectionId { get; }

		private readonly TransactionManager _txManager;

		private int _open;
		private int _closed;
		private TransactionScope? _transaction;

		public Cursor(ObjectId collectionId, TransactionManager transactionManager)
		{
			CollectionId = collectionId;
			_open = 0;
			_closed = 0;
			_txManager = transactionManager;
		}

		public void Close()
		{
			if (Interlocked.CompareExchange(ref _closed, 1, 0) == 0)
			{
				if (_transaction is not null)
				{
					_txManager.RollbackTransaction(_transaction);
				}
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

			try
			{
				_transaction = _txManager.GetAutomaticTransaction(CollectionId, TransactionMode.Read);
				foreach (var value in EnumerateValues(_transaction))
				{
					yield return value;
					if (Interlocked.CompareExchange(ref _closed, _closed, 1) == 1)
					{
						throw new BarbadosException(
							BarbadosExceptionCode.CursorClosed, $"Current cursor has been closed"
						);
					}
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

		protected abstract IEnumerable<T> EnumerateValues(TransactionScope transaction);
	}
}
