using System.Collections.Generic;

using Barbados.StorageEngine.Transactions.Locks;
using Barbados.StorageEngine.Transactions.Recovery;

namespace Barbados.StorageEngine.Transactions
{
	internal sealed class Transaction
	{
		public ObjectId Id { get; }
		public Snapshot Snapshot { get; }
		public TransactionMode Mode { get; }
		public List<Lock.AcquireScope> LockScopes { get; }
		public Stack<TransactionScope> TransactionScopes { get; }

		public Transaction(ObjectId id, Snapshot snapshot, TransactionMode mode, List<Lock.AcquireScope> scopes)
		{
			Id = id;
			Snapshot = snapshot;
			Mode = mode;
			LockScopes = scopes;
			TransactionScopes = [];
		}
	}
}
