namespace Barbados.StorageEngine
{
	public sealed class ConnectionSettings
	{
		public static ConnectionSettings Default { get; } = new ConnectionSettingsBuilder().Build();

		public required string DatabaseFilePath { get; init; }
		public required string WalFilePath { get; init; }
		public required OnConnectAction OnConnectAction { get; init; }
	}
}
