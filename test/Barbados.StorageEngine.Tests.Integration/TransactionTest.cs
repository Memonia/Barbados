using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utility;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed partial class TransactionTest : IClassFixture<BarbadosContextFixture<TransactionTest>>
	{
		[Fact]
		public void CreateTransaction_UseNotIncludedCollection_Throws()
		{
			var cname = nameof(CreateTransaction_UseNotIncludedCollection_Throws);
			_fixture.Context.Database.Collections.Create(cname);
			var collection = _fixture.Context.Database.Collections.Get(cname);

			using var _ = _fixture.Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.BeginTransaction();

			Assert.Throws<BarbadosException>(() => collection.GetCursor().ToList());
		}

		[Fact]
		public void CreateTransaction_CreateSecondTransaction_Throws()
		{
			using var _ = _fixture.Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.BeginTransaction();

			Assert.Throws<BarbadosException>(() =>
			{
				using var _ = _fixture.Context.Database.CreateTransaction(TransactionMode.ReadWrite)
					.BeginTransaction();
			});
		}

		[Fact]
		public void NoTransactionCreated_CommitTransaction_Throws()
		{
			Assert.Throws<BarbadosException>(() => _fixture.Context.Database.CommitTransaction());
		}

		[Fact]
		public void NoTransactionCreated_RollbackTransaction_Throws()
		{
			Assert.Throws<BarbadosException>(() => _fixture.Context.Database.RollbackTransaction());
		}

		[Fact]
		public void StartTransaction_CommitTransaction_ChangesPersist()
		{
			var cname = nameof(StartTransaction_CommitTransaction_ChangesPersist);
			_fixture.Context.Database.Collections.Create(cname);
			var collection = _fixture.Context.Database.Collections.Get(cname);
			var doc = new BarbadosDocument.Builder()
				.Add("test-field", 1)
				.Build();

			using var _ = _fixture.Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.Include(collection)
				.BeginTransaction();

			var id = collection.Insert(doc);

			_fixture.Context.Database.CommitTransaction();

			var r = collection.TryRead(id, out var rdoc);

			Assert.True(r);
			Assert.Equal(id, rdoc.Id);
			Assert.Equal(doc.Count(), rdoc.Count());
			Assert.Single(collection.GetCursor());
		}

		[Fact]
		public void StartTransaction_ThrowException_ChangesReverted()
		{
			var cname = nameof(StartTransaction_ThrowException_ChangesReverted);
			_fixture.Context.Database.Collections.Create(cname);
			var collection = _fixture.Context.Database.Collections.Get(cname);
			var doc = new BarbadosDocument.Builder()
				.Add("test-field", 1)
				.Build();

			var id = ObjectId.Invalid;
			try
			{
				using var _ = _fixture.Context.Database.CreateTransaction(TransactionMode.ReadWrite)
					.Include(collection)
					.BeginTransaction();

				id = collection.Insert(doc);
				throw new Exception();
			}

			catch
			{
				var r = collection.TryRead(id, out var rdoc);
				Assert.False(r);
				Assert.Empty(collection.GetCursor());
			}
		}

		[Fact]
		public void StartTransaction_RollbackTransaction_ChangesReverted()
		{
			var cname = nameof(StartTransaction_RollbackTransaction_ChangesReverted);
			_fixture.Context.Database.Collections.Create(cname);
			var collection = _fixture.Context.Database.Collections.Get(cname);
			var doc = new BarbadosDocument.Builder()
				.Add("test-field", 1)
				.Build();

			using var _ = _fixture.Context.Database.CreateTransaction(TransactionMode.ReadWrite)
				.Include(collection)
				.BeginTransaction();

			var id = collection.Insert(doc);

			_fixture.Context.Database.RollbackTransaction();

			var r = collection.TryRead(id, out var rdoc);

			Assert.False(r);
			Assert.Empty(collection.GetCursor());
		}

		[Fact]
		public async Task StartTransactions_StartSecondTransactionWithTimeout_SecondTransactionsTimesOut()
		{
			var cname = nameof(StartTransactions_StartSecondTransactionWithTimeout_SecondTransactionsTimesOut);
			_fixture.Context.Database.Collections.Create(cname);
			var collection = _fixture.Context.Database.Collections.Get(cname);

			var t1ExitSync = new ManualResetEventSlim(false);
			var t2ExitSync = new ManualResetEventSlim(false);
			var t1 = new Task(() =>
			{
				using var _ = _fixture.Context.Database.CreateTransaction(TransactionMode.ReadWrite)
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
						using var _ = _fixture.Context.Database.CreateTransaction(TransactionMode.ReadWrite)
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
