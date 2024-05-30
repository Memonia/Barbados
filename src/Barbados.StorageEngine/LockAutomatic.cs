namespace Barbados.StorageEngine
{
	internal sealed partial class LockAutomatic
	{
		public string Name { get; }

		private readonly LockManager _manager;

		public LockAutomatic(string name, LockManager manager)
		{
			Name = name;
			_manager = manager;
		}

		public Scope Acquire(LockMode mode)
		{
			_manager.Acquire(Name, mode);
			return new Scope();
		}

		public void Release(LockMode mode)
		{
			_manager.Release(Name, mode);
		}
	}
}
