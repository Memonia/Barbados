using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utils;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed class TransactionTest : SetupTeardownBarbadosContextTestClass<TransactionTest>
	{
		[Test]
		public void CreateTransaction_UseNotIncludedCollection_Throws()
		{
			var cname = nameof(CreateTransaction_UseNotIncludedCollection_Throws);
			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);

			using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.BeginTransaction();

			Assert.Throws<BarbadosException>(() => collection.Find(FindOptions.All).FirstOrDefault());
		}

		[Test]
		public void CreateTransaction_CreateSecondTransaction_Throws()
		{
			using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.BeginTransaction();

			Assert.Throws<BarbadosException>(() =>
			{
				using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
					.BeginTransaction();
			});
		}

		[Test]
		public void NoTransactionCreated_CommitTransaction_Throws()
		{
			Assert.Throws<BarbadosException>(() => Context.Database.CommitTransaction());
		}

		[Test]
		public void NoTransactionCreated_RollbackTransaction_Throws()
		{
			Assert.Throws<BarbadosException>(() => Context.Database.RollbackTransaction());
		}

		[Test]
		public void StartTransaction_CommitTransaction_ChangesPersist()
		{
			var cname = nameof(StartTransaction_CommitTransaction_ChangesPersist);
			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);
			var doc = new BarbadosDocument.Builder()
				.Add(BarbadosDocumentKeys.DocumentId, 1)
				.Build();

			using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.Include(collection)
				.BeginTransaction();

			collection.Insert(doc);

			Context.Database.CommitTransaction();

			var found = collection.Find(FindOptions.Single(1)).ToList();
			Assert.That(found, Has.Exactly(1).Items);
			Assert.That(found[0].Count(), Is.EqualTo(doc.Count()));
		}

		[Test]
		public void StartTransaction_RollbackTransaction_ChangesReverted()
		{
			var cname = nameof(StartTransaction_RollbackTransaction_ChangesReverted);
			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);
			var doc = new BarbadosDocument.Builder()
				.Add(BarbadosDocumentKeys.DocumentId, 1)
				.Build();

			using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.Include(collection)
				.BeginTransaction();

			collection.Insert(doc);

			Context.Database.RollbackTransaction();

			var found = collection.Find(FindOptions.All).ToList();
			Assert.That(found, Has.Exactly(0).Items);
		}

		[Test]
		public void StartTransaction_ThrowException_ChangesReverted()
		{
			var cname = nameof(StartTransaction_ThrowException_ChangesReverted);
			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);
			var doc = new BarbadosDocument.Builder()
				.Add(BarbadosDocumentKeys.DocumentId, 1)
				.Build();

			try
			{
				using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
					.Include(collection)
					.BeginTransaction();

				collection.Insert(doc);
				throw new Exception();
			}

			catch
			{
				var found = collection.Find(FindOptions.All).ToList();
				Assert.That(found, Has.Exactly(0).Items);
			}
		}

		[Test]
		public async Task StartTransactions_StartSecondTransactionWithTimeout_SecondTransactionsTimesOut()
		{
			var cname = nameof(StartTransactions_StartSecondTransactionWithTimeout_SecondTransactionsTimesOut);
			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);

			var t1ExitSync = new ManualResetEventSlim(false);
			var t2ExitSync = new ManualResetEventSlim(false);
			var t1 = new Task(() =>
			{
				using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
					.Include(collection)
					.BeginTransaction();

				t1ExitSync.Set();
				t2ExitSync.Wait();
			});

			var t2 = new Task(() =>
			{
				t1ExitSync.Wait();
				try
				{
					Assert.Throws<TimeoutException>(() =>
					{
						using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
							.Include(collection)
							.BeginTransaction(TimeSpan.FromSeconds(1));
					});
				}

				catch
				{
					t2ExitSync.Set();
					throw;
				}

				t2ExitSync.Set();
			});

			t1.Start();
			t2.Start();
			await t2;
			await t1;
		}
	}
}
