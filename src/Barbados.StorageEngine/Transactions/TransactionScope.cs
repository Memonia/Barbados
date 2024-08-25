using System;

using Barbados.StorageEngine.Transactions.Recovery;

namespace Barbados.StorageEngine.Transactions
{
	internal sealed partial class TransactionScope : ITransaction
	{
		public event Action<TransactionScope>? OnCompleted;

		public Snapshot Snapshot { get; }
		public TransactionMode Mode { get; }
		public bool IsActive { get;set; }

		private readonly WalBuffer _wal;

		public TransactionScope(Snapshot snapshot, TransactionMode mode, WalBuffer wal)
		{
			Mode = mode;
			Snapshot = snapshot;
			IsActive = true;
			_wal = wal;
		}

		public void Dispose()
		{
			if (IsActive)
			{
				OnCompleted?.Invoke(this);
			}
		}

		private void _throwModeMismatch(TransactionMode mode)
		{
			if (Mode != mode)
			{
				throw new InvalidOperationException($"Cannot perform current operation in '{Mode}' transaction mode");
			}
		}
	}
}
