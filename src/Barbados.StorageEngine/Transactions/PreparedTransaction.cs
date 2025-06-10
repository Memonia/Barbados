using System;

namespace Barbados.StorageEngine.Transactions
{
	internal sealed class PreparedTransaction
	{
		private readonly TransactionBuilder _builder;

		public PreparedTransaction(TransactionBuilder builder)
		{
			_builder = builder;
		}

		public TransactionScope Begin()
		{
			return _builder.BeginTransaction();
		}

		public TransactionScope Begin(TimeSpan timeout)
		{
			return _builder.BeginTransaction(timeout);
		}
	}
}
