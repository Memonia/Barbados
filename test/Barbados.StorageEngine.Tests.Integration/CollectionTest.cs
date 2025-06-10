using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration
{
	public sealed partial class CollectionTest : SetupTeardownBarbadosContextTestClass<CollectionTest>
	{
		[Test]
		public void CreateCollection_GetInstance_DeleteCollection_AnyInstanceMethodThrows()
		{
			var cname = nameof(CreateCollection_GetInstance_DeleteCollection_AnyInstanceMethodThrows);
			var iname = "index";
			Context.Database.Collections.Create(cname);
			Context.Database.Indexes.Create(cname, iname);
			var collection = Context.Database.Collections.Get(cname);
			Context.Database.Collections.Delete(cname);

			var doc = new BarbadosDocument.Builder()
				.Add(BarbadosDocumentKeys.DocumentId, 1)
				.Add("index", 2)
				.Build();

			Assert.Multiple(() =>
			{
				Assert.That(() => collection.InsertWithAutomaticId(new()), Throws.Exception);
				Assert.That(() => collection.Insert(doc), Throws.Exception);
				Assert.That(() => collection.Update(doc), Throws.Exception);
				Assert.That(() => collection.Remove(doc), Throws.Exception);
				Assert.That(() => collection.TryInsert(doc), Throws.Exception);
				Assert.That(() => collection.TryUpdate(doc), Throws.Exception);
				Assert.That(() => collection.TryRemove(doc), Throws.Exception);
				Assert.That(() => collection.Find(FindOptions.All).ToList(), Throws.Exception);
				Assert.That(() => collection.Find(FindOptions.All, iname).ToList(), Throws.Exception);
			});
		}
	}
}
