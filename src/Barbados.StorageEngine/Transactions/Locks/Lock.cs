using System;

namespace Barbados.StorageEngine.Transactions.Locks
{
	internal sealed partial class Lock
	{
		public ObjectId Id { get; }

		private readonly LockManager _manager;

		public Lock(ObjectId id, LockManager manager)
		{
			Id = id;
			_manager = manager;
		}

		public AcquireScope Acquire(LockMode mode)
		{
			_manager.Acquire(Id, mode);
			return new AcquireScope(this, mode);
		}

		public bool TryAcquire(LockMode mode, TimeSpan timeout, out AcquireScope scope)
		{
			if (_manager.TryAcquire(Id, mode, timeout))
			{
				scope = new AcquireScope(this, mode);
				return true;
			}

			scope = default!;
			return false;
		}

		public void Release(LockMode mode)
		{
			_manager.Release(Id, mode);
		}
	}
}
