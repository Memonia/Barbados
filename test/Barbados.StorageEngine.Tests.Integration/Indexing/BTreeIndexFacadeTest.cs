using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.Tests.Integration.Utility;
using Barbados.StorageEngine.Transactions;
using Barbados.StorageEngine.Transactions.Locks;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public sealed partial class BTreeIndexFacadeTest
	{
		public sealed class Insert : SetupTeardownBarbadosContextTestClass<Insert>
		{
			[Test]
			[TestCaseSource(typeof(BTreeIndexFacadeTestSequenceProvider))]
			public void InsertSequence_ReadEverythingBackSuccess(BTreeIndexFacadeTestSequence sequence)
			{
				var name = $"{nameof(InsertSequence_ReadEverythingBackSuccess)}-{sequence.DocumentSequence.Name}";
				var index = Context.CreateTestBTreeIndexFacade(name, sequence);
				var collection = Context.GetTestBarbadosCollectionFacade(name);

				using var insertTransaction = Context.DatabaseFacade.TransactionManager
					.CreateTransaction(TransactionMode.ReadWrite)
					.IncludeLock(collection.Id, LockMode.Write)
					.BeginTransaction();

				var ids = new Dictionary<object, List<ObjectId>>();
				foreach (var document in sequence.DocumentSequence.Documents)
				{
					var r = document.TryGet(sequence.IndexField, out var key);
					Debug.Assert(r);

					var id = collection.Insert(document);
					if (ids.TryGetValue(key, out var existingIds))
					{
						existingIds.Add(id);
					}

					else
					{
						ids.Add(key, [id]);
					}
				}

				Context.DatabaseFacade.TransactionManager.CommitTransaction(insertTransaction); 
				using var readTransaction = Context.DatabaseFacade.TransactionManager
					.CreateTransaction(TransactionMode.Read)
					.IncludeLock(collection.Id, LockMode.Read)
					.BeginTransaction();

				foreach (var (key, expectedIds) in ids)
				{
					var foundIds = index.FindExact(key).ToList();
					Assert.That(expectedIds, Is.EquivalentTo(foundIds));
				}
			}
		}

		public sealed class TryRemove : SetupTeardownBarbadosContextTestClass<TryRemove>
		{
			[Test]
			[TestCaseSource(typeof(BTreeIndexFacadeTestSequenceProvider))]
			public void InsertSequenceThenRemoveAll_EverythingRemovedSuccess(BTreeIndexFacadeTestSequence sequence)
			{
				var name = $"{nameof(InsertSequenceThenRemoveAll_EverythingRemovedSuccess)}-{sequence.DocumentSequence.Name}";
				var index = Context.CreateTestBTreeIndexFacade(name, sequence);
				var collection = Context.GetTestBarbadosCollectionFacade(name);

				using var insertTransaction = Context.DatabaseFacade.TransactionManager
					.CreateTransaction(TransactionMode.ReadWrite)
					.IncludeLock(collection.Id, LockMode.Write)
					.BeginTransaction();

				var ids = new Dictionary<object, List<ObjectId>>();
				foreach (var document in sequence.DocumentSequence.Documents)
				{
					var r = document.TryGet(sequence.IndexField, out var key);
					Debug.Assert(r);

					var id = collection.Insert(document);
					if (ids.TryGetValue(key, out var existingIds))
					{
						existingIds.Add(id);
					}

					else
					{
						ids.Add(key, [id]);
					}
				}

				Context.DatabaseFacade.TransactionManager.CommitTransaction(insertTransaction);
				using var removeTransaction = Context.DatabaseFacade.TransactionManager
					.CreateTransaction(TransactionMode.ReadWrite)
					.IncludeLock(collection.Id, LockMode.Write)
					.BeginTransaction();

				foreach (var (key, idList) in ids)
				{
					foreach (var id in idList)
					{
						var r = collection.TryRemove(id);
						Assert.That(r, Is.True);
					}
				}

				Context.DatabaseFacade.TransactionManager.CommitTransaction(removeTransaction);
				using var readTransaction = Context.DatabaseFacade.TransactionManager
					.CreateTransaction(TransactionMode.Read)
					.IncludeLock(collection.Id, LockMode.Read)
					.BeginTransaction();

				foreach (var (key, __REMOVE_ID) in ids)
				{
					var foundIds = index.FindExact(key).ToList();
					Assert.That(foundIds, Is.Empty);
				}
			}
		}
	}
}
