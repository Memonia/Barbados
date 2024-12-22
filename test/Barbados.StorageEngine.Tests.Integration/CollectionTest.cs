using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed partial class CollectionTest : SetupTeardownBarbadosContextTestClass<CollectionTest>
	{
		[Test]
		public void CreateCollection_InsertSome_CreateIndex_IndexBuiltCorrectly()
		{
			var values = new int[] { 1, 2, 3, 4, 5, 6, 7 };
			var fname = "test-field";
			var cname = nameof(CreateCollection_InsertSome_CreateIndex_IndexBuiltCorrectly);

			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);
			var db = new BarbadosDocument.Builder();
			var ids = new ObjectId[values.Length];
			for (int i = 0; i < values.Length; ++i)
			{
				var value = values[i];
				var doc = db.Add(fname, value).Build();
				ids[i] = collection.Insert(doc);
			}

			Context.Database.Indexes.Create(cname, fname);
			var index = Context.Database.Indexes.Get(cname, fname);
			for (int i = 0; i < values.Length; ++i)
			{
				var value = values[i];
				var foundIds = index.FindExact(value).ToArray();
				Assert.That(foundIds, Has.Exactly(1).Count);
				Assert.That(foundIds[0], Is.EqualTo(ids[i]));
			}
		}
	
		[Test]
		public void CreateIndex_GetInstance_DeleteIndex_InstanceMethodsThrow()
		{
			var fname = "test-field";
			var cname = nameof(CreateIndex_GetInstance_DeleteIndex_InstanceMethodsThrow);
			Context.Database.Collections.Create(cname);
			Context.Database.Indexes.Create(cname, fname);
			var index = Context.Database.Indexes.Get(cname, fname);
			Context.Database.Indexes.Delete(cname, fname);

			var cond = new BarbadosDocument.Builder()
				.Add(BarbadosDocumentKeys.IndexQuery.Exact, true)
				.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, 0)
				.Build();

			Assert.Throws<BarbadosConcurrencyException>(() => index.Find(cond));
			Assert.Throws<BarbadosConcurrencyException>(() => index.FindExact(0));
		}

		[Test]
		public void CreateIndex_GetInstance_DeleteCollection_InstanceMethodThrows()
		{
			var fname = "test-field";
			var cname = nameof(CreateIndex_GetInstance_DeleteCollection_InstanceMethodThrows);
			Context.Database.Collections.Create(cname);
			Context.Database.Indexes.Create(cname, fname);
			var index = Context.Database.Indexes.Get(cname, fname);
			Context.Database.Collections.Delete(cname);

			var cond = new BarbadosDocument.Builder()
				.Add(BarbadosDocumentKeys.IndexQuery.Exact, true)
				.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, 0)
				.Build();

			Assert.Throws<BarbadosConcurrencyException>(() => index.Find(cond));
			Assert.Throws<BarbadosConcurrencyException>(() => index.FindExact(0));
		}

		[Test]
		public void CreateCollection_GetInstance_DeleteCollection_InstanceMethodsThrow()
		{
			var cname = nameof(CreateCollection_GetInstance_DeleteCollection_InstanceMethodsThrow);
			Context.Database.Collections.Create(cname);
			var collection = Context.Database.Collections.Get(cname);
			Context.Database.Collections.Delete(cname);

			Assert.Throws<BarbadosConcurrencyException>(() => collection.Name.ToString());
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
