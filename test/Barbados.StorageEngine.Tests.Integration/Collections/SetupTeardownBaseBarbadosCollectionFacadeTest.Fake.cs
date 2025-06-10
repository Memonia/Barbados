using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	internal partial class SetupTeardownBaseBarbadosCollectionFacadeTest<T>
	{
		private sealed class BaseBarbadosCollectionFacadeTestFake : BaseBarbadosCollectionFacade
		{
			public override BarbadosDbObjectName Name { get; }
			public required Dictionary<BarbadosKey, IndexInfo> Indexes { get; init; }

			public BaseBarbadosCollectionFacadeTestFake(BarbadosDbObjectName name, CollectionInfo info, TransactionManager transactionManager)
				: base(info, transactionManager)
			{
				Name = name;
			}

			protected override IEnumerable<IndexInfo> EnumerateIndexes()
			{
				return Indexes.Values;
			}

			protected override IndexInfo GetIndexInfo(BarbadosKey field)
			{
				if (!Indexes.TryGetValue(field, out var info))
				{
					BarbadosCollectionExceptionHelpers.ThrowIndexDoesNotExist(Id, field.ToString());
				}

				return info!;
			}
		}
	}
}
