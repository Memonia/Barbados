using System;

namespace Barbados.StorageEngine
{
	internal sealed class ObjectLock : IDisposable
	{
		public string Name { get; }
		public LockMode Mode { get; }

		private LockManager _manager { get; }

		public ObjectLock(string name, LockMode mode, LockManager manager)
		{
			Name = name;
			Mode = mode;
			_manager = manager;
		}

		public void Acquire()
		{
			_manager.Acquire(Name, Mode);
		}

		public void Dispose()
		{
			_manager.Release(Name, Mode);
		}
	}
}
