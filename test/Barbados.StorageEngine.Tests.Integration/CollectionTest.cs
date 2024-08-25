using System.Linq;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed partial class CollectionTest : IClassFixture<BarbadosContextFixture<CollectionTest>>
	{
		[Fact]
		public void CreateCollection_InsertSome_CreateIndex_IndexBuiltCorrectly()
		{
			var values = new int[] { 1, 2, 3, 4, 5, 6, 7 };
			var fname = "test-field";
			var cname = nameof(CreateCollection_InsertSome_CreateIndex_IndexBuiltCorrectly);

			_fixture.Context.Database.Collections.Create(cname);
			var collection = _fixture.Context.Database.Collections.Get(cname);
			var db = new BarbadosDocument.Builder();
			var ids = new ObjectId[values.Length];
			for (int i = 0; i < values.Length; ++i)
			{
				var value = values[i];
				var doc = db.Add(fname, value).Build();
				ids[i] = collection.Insert(doc);
			}

			_fixture.Context.Database.Indexes.Create(cname, fname);
			var index = _fixture.Context.Database.Indexes.Get(cname, fname);
			for (int i = 0; i < values.Length; ++i)
			{
				var value = values[i];
				var foundIds = index.FindExact(value).ToArray();
				Assert.Single(foundIds);
				Assert.Equal(ids[i], foundIds[0]);
			}
		}
	
		[Fact]
		public void CreateIndex_GetInstance_DeleteIndex_InstanceMethodsThrow()
		{
			var fname = "test-field";
			var cname = nameof(CreateIndex_GetInstance_DeleteIndex_InstanceMethodsThrow);
			_fixture.Context.Database.Collections.Create(cname);
			_fixture.Context.Database.Indexes.Create(cname, fname);
			var index = _fixture.Context.Database.Indexes.Get(cname, fname);
			_fixture.Context.Database.Indexes.Delete(cname, fname);

			var cond = new BarbadosDocument.Builder()
				.Add(CommonIdentifiers.Index.Exact, true)
				.Add(CommonIdentifiers.Index.SearchValue, 0)
				.Build();

			Assert.Throws<BarbadosConcurrencyException>(() => index.Find(cond));
			Assert.Throws<BarbadosConcurrencyException>(() => index.FindExact(0));
		}

		[Fact]
		public void CreateIndex_GetInstance_DeleteCollection_InstanceMethodThrows()
		{
			var fname = "test-field";
			var cname = nameof(CreateIndex_GetInstance_DeleteCollection_InstanceMethodThrows);
			_fixture.Context.Database.Collections.Create(cname);
			_fixture.Context.Database.Indexes.Create(cname, fname);
			var index = _fixture.Context.Database.Indexes.Get(cname, fname);
			_fixture.Context.Database.Collections.Delete(cname);

			var cond = new BarbadosDocument.Builder()
				.Add(CommonIdentifiers.Index.Exact, true)
				.Add(CommonIdentifiers.Index.SearchValue, 0)
				.Build();

			Assert.Throws<BarbadosConcurrencyException>(() => index.Find(cond));
			Assert.Throws<BarbadosConcurrencyException>(() => index.FindExact(0));
		}

		[Fact]
		public void CreateCollection_GetInstance_DeleteCollection_InstanceMethodsThrow()
		{
			var cname = nameof(CreateCollection_GetInstance_DeleteCollection_InstanceMethodsThrow);
			_fixture.Context.Database.Collections.Create(cname);
			var collection = _fixture.Context.Database.Collections.Get(cname);
			_fixture.Context.Database.Collections.Delete(cname);

			Assert.Throws<BarbadosConcurrencyException>(() => collection.Name);
			Assert.Throws<BarbadosConcurrencyException>(() => collection.Insert(BarbadosDocument.Empty));
			Assert.Throws<BarbadosConcurrencyException>(() => collection.TryRead(ObjectId.Invalid, out _));
			Assert.Throws<BarbadosConcurrencyException>(() => collection.Read(ObjectId.Invalid));
			Assert.Throws<BarbadosConcurrencyException>(() => collection.TryUpdate(ObjectId.Invalid, BarbadosDocument.Empty));
			Assert.Throws<BarbadosConcurrencyException>(() => collection.Update(ObjectId.Invalid, BarbadosDocument.Empty));
			Assert.Throws<BarbadosConcurrencyException>(() => collection.TryRemove(ObjectId.Invalid));
			Assert.Throws<BarbadosConcurrencyException>(() => collection.Remove(ObjectId.Invalid));
			Assert.Throws<BarbadosConcurrencyException>(() => collection.GetCursor().First());
		}
	}
}
