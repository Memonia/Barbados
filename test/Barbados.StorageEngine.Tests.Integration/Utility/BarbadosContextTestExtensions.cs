using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Tests.Integration.Indexing;

namespace Barbados.StorageEngine.Tests.Integration.Utility
{
	internal static class BarbadosContextTestExtensions
	{
		private static BTreeIndexFacade _createBTreeIndexFacade(
			BarbadosContext context, string collection, string field, int keyMaxLength, bool useDefaultLength
		)
		{
			context.DatabaseFacade.Collections.TryCreate(collection);
			if (
				!context.DatabaseFacade.Indexes.TryCreate(collection, field, keyMaxLength, useDefaultLength) ||
				!context.DatabaseFacade.Indexes.TryGet(collection, field, out var facade)
			)
			{
				throw new BarbadosInternalErrorException();
			}

			return facade;
		}

		public static BTreeIndexFacade CreateTestBTreeIndexFacade(this BarbadosContext context, string collection)
		{
			return _createBTreeIndexFacade(context, collection, "test", -1, true);
		}

		public static BTreeIndexFacade CreateTestBTreeIndexFacade(this BarbadosContext context, string collection, BTreeIndexFacadeTestSequence sequence)
		{
			return _createBTreeIndexFacade(context, collection, sequence.IndexField, sequence.KeyMaxLength, sequence.UseDefaultKeyMaxLength);
		}

		public static ManagedCollectionFacade GetTestBarbadosCollectionFacade(this BarbadosContext context, string name)
		{
			return context.DatabaseFacade.Collections.TryGet(name, out var collection)
				? collection
				: throw new BarbadosInternalErrorException();
		}

		public static ManagedCollectionFacade CreateTestBarbadosCollectionFacade(this BarbadosContext context, string name)
		{
			return
				context.DatabaseFacade.Collections.TryCreate(name) &&
				context.DatabaseFacade.Collections.TryGet(name, out var collection)
				? collection
				: throw new BarbadosInternalErrorException();
		}
	}
}
