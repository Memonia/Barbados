using System;

using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Tests.Integration.Indexing;

namespace Barbados.StorageEngine.Tests.Integration.Utils
{
	public sealed class BarbadosContextFixture<TTestClass> : IDisposable
	{
		public BarbadosContext Context => _context.Context;

		private readonly SelfCleanupBarbadosContext<TTestClass> _context;

		public BarbadosContextFixture()
		{
			_context = new SelfCleanupBarbadosContext<TTestClass>("fixture");
		}

		public void Dispose()
		{
			_context.Dispose();
		}

		internal BTreeIndex CreateTestIndex(string collection, BTreeIndexTestSequence sequence)
		{
			return _createIndex(collection, sequence.IndexedField, sequence.KeyMaxLength, sequence.UseDefaultKeyMaxLength);
		}

		internal BTreeIndex CreateTestIndex(string collection)
		{
			return _createIndex(collection, "test", -1, true);
		}

		private BTreeIndex _createIndex(string collection, string field, int keyMaxLength, bool useDefaultLength)
		{
			Context.Controller.CreateCollection(collection);
			if (useDefaultLength)
			{
				Context.Controller.CreateIndex(collection, field);
			}

			else
			{
				Context.Controller.CreateIndex(collection, field, keyMaxLength);
			}

			return Context.Controller.GetIndex(collection, field);
		}
	}
}
