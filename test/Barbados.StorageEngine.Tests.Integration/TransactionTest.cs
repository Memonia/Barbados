using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Barbados.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utility;
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

			Assert.Throws<BarbadosException>(() => collection.GetCursor().ToList());
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
				.Add("test-field", 1)
				.Build();

			using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.Include(collection)
				.BeginTransaction();

			var id = collection.Insert(doc);

			Context.Database.CommitTransaction();

			var r = collection.TryRead(id, out var rdoc);

			Assert.Multiple(() =>
			{
				Assert.That(r, Is.True);
				Assert.That(id, Is.EqualTo(rdoc.GetObjectId()));
				Assert.That(doc.Count(), Is.EqualTo(rdoc.Count()));
				Assert.That(collection.GetCursor(), Has.Exactly(1).Items);
			});
		}

		[Test]
		public void StartTransaction_ThrowException_ChangesReverted()
		{
			var cname = nameof(StartTransaction_ThrowException_ChangesReverted);
			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);
			var doc = new BarbadosDocument.Builder()
				.Add("test-field", 1)
				.Build();

			var id = ObjectId.Invalid;
			try
			{
				using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
					.Include(collection)
					.BeginTransaction();

				id = collection.Insert(doc);
				throw new Exception();
			}

			catch
			{
				var r = collection.TryRead(id, out var rdoc);
				Assert.Multiple(() =>
				{
					Assert.That(r, Is.False);
					Assert.That(collection.GetCursor(), Has.Exactly(0).Items);
				});
			}
		}

		[Test]
		public void StartTransaction_RollbackTransaction_ChangesReverted()
		{
			var cname = nameof(StartTransaction_RollbackTransaction_ChangesReverted);
			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);
			var doc = new BarbadosDocument.Builder()
				.Add("test-field", 1)
				.Build();

			using var _ = Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.Include(collection)
				.BeginTransaction();

			var id = collection.Insert(doc);

			Context.Database.RollbackTransaction();

			var r = collection.TryRead(id, out var rdoc);

			Assert.Multiple(() =>
			{
				Assert.That(r, Is.False);
				Assert.That(collection.GetCursor(), Has.Exactly(0).Items);
			});
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
