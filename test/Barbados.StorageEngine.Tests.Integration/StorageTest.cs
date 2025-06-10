using System.IO;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage;
using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed partial class StorageTest
	{
		[Test]
		public void CreateDatabase_CorruptDbRoot_CorruptionDetected()
		{
			var dbName = $"{nameof(CreateDatabase_CorruptDbRoot_CorruptionDetected)}.test-db";
			var walName = $"{nameof(CreateDatabase_CorruptDbRoot_CorruptionDetected)}_wal.test-db";
			BarbadosContext context = null!;
			try
			{
				var stgs = new ConnectionSettingsBuilder()
					.SetDatabaseFilePath(dbName)
					.SetWalFilePath(walName)
					.SetOnConnectAction(OnConnectAction.EnsureDatabaseOverwritten)
					.Build();

				using (context = new BarbadosContext(stgs))
				{
					context.Database.Collections.Create("test");
				}

				using (var db = new StorageWrapperFactory(false).Create(dbName))
				{
					var addr = PageHandle.Root.GetAddress();
					db.Write(addr, [0x1, 0x2, 0x3, 0x4, 0xA, 0xB, 0xC, 0xD]);
				}

				stgs = new ConnectionSettingsBuilder()
					.SetDatabaseFilePath(dbName)
					.SetWalFilePath(walName)
					.SetOnConnectAction(OnConnectAction.ThrowIfDatabaseNotFound)
					.Build();

				Assert.Throws<BarbadosException>(() =>
				{
					using var context = new BarbadosContext(stgs);
				});
			}

			finally
			{
				File.Delete(dbName);
				File.Delete(walName);
			}
		}
	}
}
