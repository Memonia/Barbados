using System;
using System.Transactions;

using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Tests.Integration.Utils;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Tests.Integration.BTree
{
	internal class SetupTeardownBTreeContextTest<T> : SetupTeardownTransactionManagerTestClass<T>
		where T : SetupTeardownBTreeContextTest<T>
	{
		protected BTreeContext Context => _context ?? throw new InvalidOperationException("Current instance has been disposed of");

		private BTreeContext? _context;

		[SetUp]
		protected new void Setup()
		{
			var tx = TransactionManager.CreateTransaction(TransactionMode.ReadWrite).BeginTransaction();
			var info = BTreeContext.CreateBTree(tx);
			_context = new BTreeContext(info, tx);
		}

		[TearDown]
		protected new void Teardown()
		{
			if (_context is not null)
			{
				_context.Transaction.Dispose();
				_context = null;
			}
		}
	}
}
