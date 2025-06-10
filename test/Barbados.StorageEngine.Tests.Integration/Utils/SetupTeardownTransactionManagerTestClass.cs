using System;
using System.IO;

using Barbados.StorageEngine.Caching;
using Barbados.StorageEngine.Storage;
using Barbados.StorageEngine.Storage.Wal;
using Barbados.StorageEngine.Transactions;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine.Tests.Integration.Utils
{
	[Parallelizable(ParallelScope.Self)]
	internal abstract class SetupTeardownTransactionManagerTestClass<TTestClass> where TTestClass : SetupTeardownTransactionManagerTestClass<TTestClass>
	{
		protected LockManager LockManager { get; private set; }
		protected TransactionManager TransactionManager { get; private set; }

		private string _basePath;
		private IStorageWrapper _db;
		private IStorageWrapper _wal;

		[SetUp]
		public void Setup()
		{
			_basePath = $"{typeof(TTestClass).FullName}";
			var dbp = $"{_basePath}.tdb";
			var walp = $"{_basePath}.twal";
			File.Delete(dbp);
			File.Delete(walp);

			var swf = new StorageWrapperFactory(inMemory: false);
			StorageObjectHelpers.EnsureDatabaseCreated(dbp, walp, swf);

			_db = swf.Create(dbp, @readonly: false);
			_wal = swf.Create(walp, @readonly: false);

			var cf = new CacheFactory(1024, CachingStrategy.Default);
			var wal = new WalBuffer(_db, _wal, cf, 1024, 1024);
			LockManager = new LockManager();
			TransactionManager = new TransactionManager(TimeSpan.FromSeconds(5), wal, LockManager);
		}

		[TearDown]
		public void Teardown()
		{
			_db.Dispose();
			_wal.Dispose();
			File.Delete($"{_basePath}.tdb");
			File.Delete($"{_basePath}.twal");
		}
	}
}
