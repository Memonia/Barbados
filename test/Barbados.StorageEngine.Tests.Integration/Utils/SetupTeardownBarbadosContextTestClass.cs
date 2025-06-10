using System;
using System.IO;

namespace Barbados.StorageEngine.Tests.Integration.Utils
{
	[Parallelizable(ParallelScope.Self)]
	public abstract class SetupTeardownBarbadosContextTestClass<T> where T : SetupTeardownBarbadosContextTestClass<T>
	{
		protected BarbadosContext Context => _context ?? throw new InvalidOperationException("Current instance has been disposed of");

		private BarbadosContext? _context;

		[SetUp]
		public void Setup()
		{
			var path = $"{typeof(T).FullName}";
			var cs = new ConnectionSettingsBuilder()
				.SetOnConnectAction(OnConnectAction.EnsureDatabaseOverwritten)
				.SetDatabaseFilePath($"{path}.test-db")
				.SetWalFilePath($"{path}_wal.test-db")
				.Build();

			_context = new BarbadosContext(cs);
		}

		[TearDown]
		public void Teardown()
		{
			var dbPath = Context.ConnectionSettings.DatabaseFilePath;
			var walPath = Context.ConnectionSettings.WalFilePath;

			_context!.Dispose();

			File.Delete(dbPath);
			File.Delete(walPath);
		}
	}
}
