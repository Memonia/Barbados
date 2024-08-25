using System;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Transactions
{
	public interface ITransactionBuilder
	{
		ITransactionBuilder Include(IReadOnlyBTreeIndex index);
		ITransactionBuilder Include(IBarbadosCollection collection);

		ITransaction BeginTransaction();
		ITransaction BeginTransaction(TimeSpan timeout);
	}
}