using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Collections.Extensions;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	internal partial class BaseBarbadosCollectionFacadeTest
	{
		public sealed class Find : SetupTeardownBaseBarbadosCollectionFacadeTest<Find>
		{
			private static readonly string _indexField = "index";

			[Test]
			public void All([Values(_smallCount, 104, 105, _largeCount)] int count, [Values] bool viaIndex)
			{
				if (viaIndex)
				{
					StubIndexAdd(_indexField);
				}

				var builders = _createBuilders(count, _indexField);
				var documents = new Dictionary<ObjectId, BarbadosDocument>(count);
				foreach (var builder in builders)
				{
					var doc = Fake.InsertWithAutomaticId(builder);
					documents.Add(doc.GetObjectId(), doc);
				}

				using var cursor = viaIndex ? Fake.Find(FindOptions.All, _indexField) : Fake.Find(FindOptions.All);
				foreach (var doc in cursor)
				{
					var id = doc.GetObjectId();
					var expectedDocument = documents[id];
					Assert.That(
						doc.Count(), Is.EqualTo(expectedDocument.Count()), "A document with a given id did not match expected document"
					);
				}
			}

			// TODO: more tests
		}
	}
}
