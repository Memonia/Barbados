using System;

using Barbados.StorageEngine.Collections;

namespace Barbados.StorageEngine.Transactions
{
	public interface ITransactionBuilder
	{
		ITransactionBuilder Include(IBarbadosCollection collection);

		ITransaction BeginTransaction();
		ITransaction BeginTransaction(TimeSpan timeout);
	}
}
