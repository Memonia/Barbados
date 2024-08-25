using System.IO;

namespace Barbados.StorageEngine.Configuration
{
	public sealed class ConnectionSettingsBuilder
	{
		private string? _databaseFilePath;
		private string? _walFilePath;
		private OnConnectAction _connectAction;

		public ConnectionSettingsBuilder()
		{
			_reset();
		}

		public ConnectionSettings Build()
		{
			_databaseFilePath ??= Path.GetFullPath("Barbados.db");
			_walFilePath ??= Path.GetFullPath(
				Path.GetFileNameWithoutExtension(_databaseFilePath) + "_wal" + Path.GetExtension(_databaseFilePath)
			);

			var cs = new ConnectionSettings
			{
				DatabaseFilePath = _databaseFilePath,
				WalFilePath = _walFilePath,
				OnConnectAction = _connectAction
			};

			_reset();
			return cs;
		}

		public ConnectionSettingsBuilder SetDatabaseFilePath(string databaseFilePath)
		{
			_databaseFilePath = Path.GetFullPath(databaseFilePath);
			return this;
		}

		public ConnectionSettingsBuilder SetWalFilePath(string walFilePath)
		{
			_walFilePath = Path.GetFullPath(walFilePath);
			return this;
		}

		public ConnectionSettingsBuilder SetOnConnectAction(OnConnectAction connectAction)
		{
			_connectAction = connectAction;
			return this;
		}

		private void _reset()
		{
			_databaseFilePath = null;
			_walFilePath = null;
			_connectAction = OnConnectAction.ThrowIfDatabaseNotFound;
		}
	}
}
