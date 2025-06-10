using System;

using Barbados.Documents;
using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Tests.Integration.Utils;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	internal partial class SetupTeardownBaseBarbadosCollectionFacadeTest<T> : SetupTeardownTransactionManagerTestClass<T>
		where T : SetupTeardownBaseBarbadosCollectionFacadeTest<T>
	{
		protected BaseBarbadosCollectionFacade Fake => _fake ?? throw new InvalidOperationException("Current instance has been disposed of");

		private BaseBarbadosCollectionFacadeTestFake? _fake;

		[SetUp]
		protected new void Setup()
		{
			_fake = (BaseBarbadosCollectionFacadeTestFake)Create(new(-1), new CreateCollectionOptions()
			{
				AutomaticIdGeneratorMode = AutomaticIdGeneratorMode.BetterWritePerformance
			});
		}

		protected IndexInfo StubIndexAdd(BarbadosKey field)
		{
			using var tx = TransactionManager.CreateTransaction(TransactionMode.ReadWrite).BeginTransaction();
			var info = new IndexInfo(BTreeContext.CreateBTree(tx), field);
			_fake!.Indexes.Add(field, info);
			TransactionManager.CommitTransaction(tx);
			return info;
		}

		protected BaseBarbadosCollectionFacade Create(ObjectId id, CreateCollectionOptions options)
		{
			using var tx = TransactionManager.CreateTransaction(TransactionMode.ReadWrite).BeginTransaction();
			var info = new CollectionInfo()
			{
				BTreeInfo = BTreeContext.CreateBTree(tx),
				CollectionId = id,
				AutomaticIdGeneratorMode = options.AutomaticIdGeneratorMode,
			};

			var fake = new BaseBarbadosCollectionFacadeTestFake("fake", info, TransactionManager)
			{
				Indexes = []
			};

			LockManager.CreateLock(id);
			TransactionManager.CommitTransaction(tx);
			return fake;
		}
	}
}
