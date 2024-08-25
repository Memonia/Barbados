using System;
using System.Threading;

namespace Barbados.StorageEngine.Transactions.Locks
{
	internal partial class Lock
	{
		public sealed class AcquireScope : IDisposable
        {
            public Lock Lock { get; }
            public LockMode Mode { get; }

			private int _disposed;

			public AcquireScope(Lock @lock, LockMode mode)
			{
				Mode = mode;
				Lock = @lock;

				_disposed = 0;
			}

			public void Dispose()
			{
				if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
				{
					Lock.Release(Mode);
				}
			}
		}
	}
}
