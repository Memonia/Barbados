using System;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine.Transactions
{
	internal partial class TransactionBuilder
	{
		ITransactionBuilder ITransactionBuilder.Include(IBarbadosCollection collection)
		{
			return IncludeLock(collection.Id,
				_mode switch
				{
					TransactionMode.Read => LockMode.Read,
					TransactionMode.ReadWrite => LockMode.Write,
					_ => throw new NotImplementedException()
				}
			);
		}

		ITransaction ITransactionBuilder.BeginTransaction()
		{
			return BeginTransaction();
		}

		ITransaction ITransactionBuilder.BeginTransaction(TimeSpan timeout)
		{
			return BeginTransaction(timeout);
		}
	}
}
