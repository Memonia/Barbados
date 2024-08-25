namespace Barbados.StorageEngine.Configuration
{
	public enum OnConnectAction
	{
		EnsureDatabaseCreated,
		EnsureDatabaseOverwritten,
		ThrowIfDatabaseNotFound
	}
}
