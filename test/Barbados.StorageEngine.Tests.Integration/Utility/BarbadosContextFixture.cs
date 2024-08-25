using System;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Tests.Integration.Indexing;

namespace Barbados.StorageEngine.Tests.Integration.Utility
{
	public sealed class BarbadosContextFixture<TTestClass> : IDisposable
	{
		public BarbadosContext Context => _context.Context;

		private readonly SelfCleanupBarbadosContext<TTestClass> _context;

		public BarbadosContextFixture()
		{
			_context = new("fixture");
		}

		public void Dispose()
		{
			_context.Dispose();
		}

		internal BTreeIndexFacade CreateTestBTreeIndexFacade(string collection)
		{
			return _createBTreeIndexFacade(collection, "test", -1, true);
		}

		internal BTreeIndexFacade CreateTestBTreeIndexFacade(string collection, BTreeIndexFacadeTestSequence sequence)
		{
			return _createBTreeIndexFacade(collection, sequence.IndexField, sequence.KeyMaxLength, sequence.UseDefaultKeyMaxLength);
		}

		internal BarbadosCollectionFacade GetTestBarbadosCollectionFacade(string name)
		{
			if (!Context.DatabaseFacade.Collections.TryGet(name, out var collection))
			{
				throw new BarbadosInternalErrorException();
			}

			return collection;
		}

		internal BarbadosCollectionFacade CreateTestBarbadosCollectionFacade(string name)
		{
			if (!Context.DatabaseFacade.Collections.TryCreate(name))
			{
				throw new BarbadosInternalErrorException();
			}

			if (!Context.DatabaseFacade.Collections.TryGet(name, out var collection))
			{
				throw new BarbadosInternalErrorException();
			}

			return collection;
		}

		private BTreeIndexFacade _createBTreeIndexFacade(string collection, string field, int keyMaxLength, bool useDefaultLength)
		{
			Context.DatabaseFacade.Collections.TryCreate(collection);
			if (!Context.DatabaseFacade.Indexes.TryCreate(collection, field, keyMaxLength, useDefaultLength))
			{
				throw new BarbadosInternalErrorException();
			}

			if (!Context.DatabaseFacade.Indexes.TryGet(collection, field, out var facade))
			{
				throw new BarbadosInternalErrorException();
			}

			return facade;
		}
	}
}
