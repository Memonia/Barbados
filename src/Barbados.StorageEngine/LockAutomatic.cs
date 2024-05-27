using System;
using System.Threading;

namespace Barbados.StorageEngine
{
	internal sealed class LockAutomatic : IDisposable
	{
		public string Name { get; }
		public LockMode Mode { get; }

		private int _disposed;
		private readonly LockManager _manager;

		public LockAutomatic(string name, LockMode mode, LockManager manager)
		{
			Name = name;
			Mode = mode;

			_disposed = 0;
			_manager = manager;
		}

		public void Acquire()
		{
			_manager.Acquire(Name, Mode);
		}

		public void Dispose()
		{
			if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
			{
				_manager.Release(Name, Mode);
			}
		}
	}
}
