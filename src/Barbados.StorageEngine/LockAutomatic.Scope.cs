using System.Threading;

namespace Barbados.StorageEngine
{
	internal partial class LockAutomatic
	{
		public ref struct Scope
		{
			public LockMode Mode { get; }
			public LockAutomatic Lock { get; }

			private int _disposed;

			public Scope(LockMode mode, LockAutomatic @lock)
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
