using System.IO;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage.Wal;

namespace Barbados.StorageEngine.Storage
{
	internal static class StorageObjectHelpers
	{
		public static void EnsureDatabaseCreated(string dbPath, string walPath, StorageWrapperFactory factory)
		{
			if (!File.Exists(dbPath))
			{
				if (File.Exists(walPath))
				{
					throw new BarbadosException(BarbadosExceptionCode.InvalidDatabaseState,
						"Database file does not exist, but WAL file does"
					);
				}

				using var db = factory.Create(dbPath);
				using var wal = factory.Create(walPath);
				WalBuffer.WriteWalHeader(
					wal,
					WalBuffer.AllocateRootAndGetMagic(db)
				);
			}

			else
			{
				if (!File.Exists(walPath))
				{
					using var db = factory.Create(dbPath, true);
					using var wal = factory.Create(walPath);
					WalBuffer.WriteWalHeader(db, wal);
					return;
				}
			}
		}
	}
}
