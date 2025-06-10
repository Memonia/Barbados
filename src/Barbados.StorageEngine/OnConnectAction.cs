namespace Barbados.StorageEngine
{
	public enum OnConnectAction
	{
		EnsureDatabaseCreated,
		EnsureDatabaseOverwritten,
		ThrowIfDatabaseNotFound
	}
}
