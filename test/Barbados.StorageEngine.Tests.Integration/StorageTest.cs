using System.Collections.Generic;
using System.IO;
using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.Configuration;
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

		[Test]
		public void CreateDatabase_MakeChanges_CloseConnection_OpenBack_ChangesPersisted()
		{
			var name = nameof(CreateDatabase_MakeChanges_CloseConnection_OpenBack_ChangesPersisted);
			var c1 = "collection1";
			var c2 = "collection2";
			var c3 = "collection3";
			var c1i1 = "index1";
			var c1i2 = "index2";
			var c2i1 = "index3";
			var c1i1v = 1;
			var c1i2v = 2;
			var c2i1v = 3;
			var doc1 = new BarbadosDocument.Builder()
				.Add(c1i1, c1i1v)
				.Add(c1i2, c1i2v)
				.Build();
			var doc2 = new BarbadosDocument.Builder()
				.Add(c2i1, c2i1v)
				.Build();

			var stgs = new ConnectionSettingsBuilder()
				.SetDatabaseFilePath(name)
				.SetOnConnectAction(OnConnectAction.EnsureDatabaseOverwritten)
				.Build();

			var c1Ids = new List<ObjectId>();
			var c2Ids = new List<ObjectId>();
			var c3Ids = new List<ObjectId>();
			using (var context = new BarbadosContext(stgs))
			{
				context.Database.Collections.Create(c1);
				context.Database.Collections.Create(c2);
				context.Database.Collections.Create(c3);
				context.Database.Indexes.Create(c1, c1i1);
				context.Database.Indexes.Create(c1, c1i2);
				context.Database.Indexes.Create(c2, c2i1);

				var c1Instance = context.Database.Collections.Get(c1);
				var c2Instance = context.Database.Collections.Get(c2);
				var c3Instance = context.Database.Collections.Get(c3);

				c1Ids.Add(c1Instance.Insert(doc1));
				c2Ids.Add(c2Instance.Insert(doc2));
				c2Ids.Add(c2Instance.Insert(doc2));
				c3Ids.Add(c3Instance.Insert(doc1));
				c3Ids.Add(c3Instance.Insert(doc1));
				c3Ids.Add(c3Instance.Insert(doc2));
			}

			stgs = new ConnectionSettingsBuilder()
				.SetDatabaseFilePath(name)
				.SetOnConnectAction(OnConnectAction.ThrowIfDatabaseNotFound)
				.Build();
			
			using (var context = new BarbadosContext(stgs))
			{
				var c1Instance = context.Database.Collections.Get(c1);
				var c2Instance = context.Database.Collections.Get(c2);
				var c3Instance = context.Database.Collections.Get(c3);
				var c1i1Instance = context.Database.Indexes.Get(c1, c1i1);
				var c1i2Instance = context.Database.Indexes.Get(c1, c1i2);
				var c2i1Instance = context.Database.Indexes.Get(c2, c2i1);

				var c1Docs = c1Instance.GetCursor().ToList();
				var c2Docs = c2Instance.GetCursor().ToList();
				var c3Docs = c3Instance.GetCursor().ToList();

				var c1i1Found = c1i1Instance.FindExact(c1i1v).ToList();
				var c1i2Found = c1i2Instance.FindExact(c1i2v).ToList();
				var c2i1Found = c2i1Instance.FindExact(c2i1v).ToList();

				Assert.Multiple(() =>
				{
					Assert.That(c1Ids, Has.Count.EqualTo(c1Docs.Count));
					Assert.That(c2Ids, Has.Count.EqualTo(c2Docs.Count));
					Assert.That(c3Ids, Has.Count.EqualTo(c3Docs.Count));
					Assert.That(c1Ids, Is.EquivalentTo(c1Docs.Select(e => e.GetObjectId())));
					Assert.That(c2Ids, Is.EquivalentTo(c2Docs.Select(e => e.GetObjectId())));
					Assert.That(c3Ids, Is.EquivalentTo(c3Docs.Select(e => e.GetObjectId())));
					Assert.That(c1i1Found, Has.Exactly(1).Count);
					Assert.That(c1i2Found, Has.Exactly(1).Count);
					Assert.That(c2i1Found, Has.Exactly(c2Ids.Count).Count);
				});
			}

			File.Delete(stgs.DatabaseFilePath);
			File.Delete(stgs.WalFilePath);
		}
	}
}
