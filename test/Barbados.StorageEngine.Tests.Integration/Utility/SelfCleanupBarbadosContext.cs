using System;
using System.IO;
using System.Runtime.CompilerServices;

using Barbados.StorageEngine.Configuration;

namespace Barbados.StorageEngine.Tests.Integration.Utility
{
	internal sealed class SelfCleanupBarbadosContext<TTestClass> : IDisposable
	{
		public BarbadosContext Context { get; }

		public SelfCleanupBarbadosContext([CallerMemberName] string caller = "")
		{
			var path = $"{typeof(TTestClass).FullName}-{caller}";
			File.Delete(path);

			var cs = new ConnectionSettingsBuilder()
				.SetOnConnectAction(OnConnectAction.EnsureDatabaseOverwritten)
				.SetDatabaseFilePath($"{path}.test-db")
				.SetWalFilePath($"{path}_wal.test-db")
				.Build();

			Context = new BarbadosContext(cs);
		}

		public void Dispose()
		{
			var dbPath = Context.ConnectionSettings.DatabaseFilePath;
			var walPath = Context.ConnectionSettings.WalFilePath;
			Context.Dispose();
			File.Delete(dbPath);
			File.Delete(walPath);
		}
	}
}
