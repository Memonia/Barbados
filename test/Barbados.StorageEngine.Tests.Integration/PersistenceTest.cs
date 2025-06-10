using System.IO;
using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration
{
	internal sealed class PersistenceTest : SetupTeardownBarbadosContextTestClass<PersistenceTest>
	{
		[Test]
		public void CreateDatabase_MakeChanges_CloseConnection_OpenConnection_ChangesPersist()
		{
			var name = nameof(CreateDatabase_MakeChanges_CloseConnection_OpenConnection_ChangesPersist);
			var c1 = "collection1";
			var c2 = "collection2";
			var c1i1 = "index1";
			var c1i2 = "index2";
			var c2i1 = "index3";
			var c1i1v = 1;
			var c1i2v = 2;
			var c2i1v = 3;
			var doc1 = new BarbadosDocument.Builder()
				.Add(BarbadosDocumentKeys.DocumentId, 1)
				.Add(c1i1, c1i1v)
				.Add(c1i2, c1i2v)
				.Build();
			var doc2 = new BarbadosDocument.Builder()
				.Add(BarbadosDocumentKeys.DocumentId, 2)
				.Add(c2i1, c2i1v)
				.Build();

			var stgs = new ConnectionSettingsBuilder()
				.SetDatabaseFilePath(name)
				.SetOnConnectAction(OnConnectAction.EnsureDatabaseOverwritten)
				.Build();

			using (var context = new BarbadosContext(stgs))
			{
				context.Database.Collections.Create(c1);
				context.Database.Collections.Create(c2);
				context.Database.Indexes.Create(c1, c1i1);
				context.Database.Indexes.Create(c1, c1i2);
				context.Database.Indexes.Create(c2, c2i1);

				var c1Instance = context.Database.Collections.Get(c1);
				var c2Instance = context.Database.Collections.Get(c2);

				c1Instance.Insert(doc1);
				c1Instance.Insert(doc2);
				c2Instance.Insert(doc1);
				c2Instance.Insert(doc2);
			}

			stgs = new ConnectionSettingsBuilder()
				.SetDatabaseFilePath(name)
				.SetOnConnectAction(OnConnectAction.ThrowIfDatabaseNotFound)
				.Build();

			using (var context = new BarbadosContext(stgs))
			{
				var c1Instance = context.Database.Collections.Get(c1);
				var c2Instance = context.Database.Collections.Get(c2);

				var c1Docs = c1Instance.Find(FindOptions.All).ToList();
				var c2Docs = c2Instance.Find(FindOptions.All).ToList();

				var c1i1Found = c1Instance.Find(FindOptions.All, c1i1).ToList();
				var c1i2Found = c1Instance.Find(FindOptions.All, c1i2).ToList();
				var c2i1Found = c2Instance.Find(FindOptions.All, c2i1).ToList();

				Assert.Multiple(() =>
				{
					Assert.That(c1Docs, Has.Count.EqualTo(2));
					Assert.That(c2Docs, Has.Count.EqualTo(2));
					Assert.That(c1i1Found, Has.Count.EqualTo(1));
					Assert.That(c1i2Found, Has.Count.EqualTo(1));
					Assert.That(c2i1Found, Has.Count.EqualTo(1));
				});
			}

			File.Delete(stgs.DatabaseFilePath);
			File.Delete(stgs.WalFilePath);
		}
	}
}
