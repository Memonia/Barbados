﻿using System;
using System.IO;

using Barbados.StorageEngine.Caching;
using Barbados.StorageEngine.Storage;
using Barbados.StorageEngine.Storage.Wal;

namespace Barbados.StorageEngine
{
	public sealed class BarbadosContext : IDisposable
	{
		public IDatabaseFacade Database => DatabaseFacade;

		public StorageOptions StorageOptions { get; }
		public ConnectionSettings ConnectionSettings { get; }

		internal DatabaseFacade DatabaseFacade { get; }

		private readonly IStorageWrapper _db;
		private readonly IStorageWrapper _wal;

		public BarbadosContext(string path) : this(path, StorageOptions.Default)
		{

		}

		public BarbadosContext(string path, StorageOptions storageOptions) :
			this(
				new ConnectionSettingsBuilder().SetDatabaseFilePath(path).Build(), storageOptions
			)
		{

		}

		public BarbadosContext(ConnectionSettings connectionSettings) : this(connectionSettings, StorageOptions.Default)
		{

		}

		public BarbadosContext(ConnectionSettings connectionSettings, StorageOptions storageOptions)
		{
			var factory = new StorageWrapperFactory(false);
			switch (connectionSettings.OnConnectAction)
			{
				case OnConnectAction.EnsureDatabaseCreated:
					StorageObjectHelpers.EnsureDatabaseCreated(
						connectionSettings.DatabaseFilePath, connectionSettings.WalFilePath, factory
					);
					break;

				case OnConnectAction.EnsureDatabaseOverwritten:
					File.Delete(connectionSettings.WalFilePath);
					File.Delete(connectionSettings.DatabaseFilePath);
					StorageObjectHelpers.EnsureDatabaseCreated(
						connectionSettings.DatabaseFilePath, connectionSettings.WalFilePath, factory
					);
					break;

				case OnConnectAction.ThrowIfDatabaseNotFound:
					if (!File.Exists(connectionSettings.DatabaseFilePath))
					{
						throw new FileNotFoundException(
							"Database file not found", connectionSettings.DatabaseFilePath
						);
					}

					StorageObjectHelpers.EnsureDatabaseCreated(
						connectionSettings.DatabaseFilePath, connectionSettings.WalFilePath, factory
					);
					break;

				default:
					throw new NotImplementedException();
			}

			_db = factory.Create(connectionSettings.DatabaseFilePath);
			_wal = factory.Create(connectionSettings.WalFilePath);

			WalBuffer walBuffer;
			try
			{
				walBuffer = new WalBuffer(
					_db,
					_wal,
					new CacheFactory(
						storageOptions.CachedPageCountLimit,
						storageOptions.CachingStrategy
					),
					storageOptions.WalPageCountLimit,
					storageOptions.WalBufferedPageCountLimit
				);

				walBuffer.Restore();
			}

			catch
			{
				_db.Dispose();
				_wal.Dispose();
				throw;
			}

			DatabaseFacade = new DatabaseFacade(walBuffer, storageOptions.TransactionAcquireLockTimeout);
			StorageOptions = storageOptions;
			ConnectionSettings = connectionSettings;	
		}

		public void Dispose()
		{
			_db.Dispose();
			_wal.Dispose();
		}
	}
}
