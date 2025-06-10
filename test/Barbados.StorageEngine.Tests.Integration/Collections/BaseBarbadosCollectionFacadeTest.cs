using System;
using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	internal sealed partial class BaseBarbadosCollectionFacadeTest
	{
		private const int _smallCount = 16;
		private const int _largeCount = 128;

		public sealed class Deallocate : SetupTeardownBaseBarbadosCollectionFacadeTest<Deallocate>
		{
			[Test]
			public void CreateMultipleIndexes_Deallocate_NoErrors()
			{
				var indexFields = new[] { "index1", "index2", "index3" };
				foreach (var field in indexFields)
				{
					StubIndexAdd(field);
				}

				Fake.Deallocate();
			}
		}

		public sealed class IndexBuild : SetupTeardownBaseBarbadosCollectionFacadeTest<IndexBuild>
		{
			[Test]
			public void InsertDocuments_IndexBuild_FindAll_ViaIndex_CorrectCountFound()
			{
				var indexField = "index-field";
				var count = _smallCount;
				var viaIndexCount = count / 2;

				StubIndexAdd(indexField);

				var builders = _createBuilders(count);
				foreach (var (index, builder) in builders.Index())
				{
					if ((index & 1) == 0)
					{
						builder.Add(indexField, index);
					}

					Fake.InsertWithAutomaticId(builder);
				}

				using var cursor = Fake.Find(FindOptions.All, indexField);
				Assert.That(cursor.Count(), Is.EqualTo(viaIndexCount), "Not all documents were found via index");

			}

			[Test]
			public void InsertDocuments_ZeroIndexedFields_IndexBuild_FindAll_ViaIndex_NoDocumentsFound()
			{
				var indexField = "index-field";
				var count = _smallCount;
				var viaIndexCount = 0;

				StubIndexAdd(indexField);

				var builders = _createBuilders(count);
				foreach (var builder in builders)
				{
					Fake.InsertWithAutomaticId(builder);
				}

				using var cursor = Fake.Find(FindOptions.All, indexField);
				Assert.That(cursor.Count(), Is.EqualTo(viaIndexCount), "No documents should be found via index");
			}
		}

		public sealed class IndexDeallocate : SetupTeardownBaseBarbadosCollectionFacadeTest<IndexDeallocate>
		{
			[Test]
			public void CreateMultipleIndexes_DeallocateSingle_NoErrors()
			{
				var indexFields = new[] { "index1", "index2", "index3" };
				var indexInfos = new IndexInfo[indexFields.Length];
				foreach (var (index, field) in indexFields.Index())
				{
					indexInfos[index] = StubIndexAdd(field);
				}

				Fake.IndexDeallocate(indexInfos[0]);
				Fake.IndexDeallocate(indexInfos[1]);
			}
		}

		public sealed class InsertWithAutomaticId : SetupTeardownBaseBarbadosCollectionFacadeTest<InsertWithAutomaticId>
		{
			[Test]
			public void InsertDocuments_UniquePrimaryKeyValuesAssigned([Values] AutomaticIdGeneratorMode mode)
			{
				var fake = Create(new(1), new CreateCollectionOptions() { AutomaticIdGeneratorMode = mode });
				var count = _smallCount;
				var builders = _createBuilders(count);
				var ids = new long[count];
				foreach (var (index, builder) in builders.Index())
				{
					var doc = fake.InsertWithAutomaticId(builder);
					ids[index] = doc.GetInt64(BarbadosDocumentKeys.DocumentId);
				}

				Assert.That(ids, Is.Unique);
			}
		}

		public sealed class Insert : SetupTeardownBaseBarbadosCollectionFacadeTest<Insert>
		{
			[Test]
			public void NoPrimaryKey_Throws()
			{
				var doc = new BarbadosDocument.Builder().Add("t", 1).Build();
				Assert.That(() => Fake.Insert(doc), Throws.Exception);
			}
		}

		public sealed class Remove : SetupTeardownBaseBarbadosCollectionFacadeTest<Remove>
		{
			[Test]
			public void NoDocumentWithGivenPrimaryKey_Throws()
			{
				var doc = new BarbadosDocument.Builder().Add(BarbadosDocumentKeys.DocumentId, 1).Build();
				Assert.That(() => Fake.Remove(doc), Throws.Exception);
			}
		}

		public sealed class Update : SetupTeardownBaseBarbadosCollectionFacadeTest<Update>
		{
			[Test]
			public void NoDocumentWithGivenPrimaryKey_Throws()
			{
				var doc = new BarbadosDocument.Builder().Add(BarbadosDocumentKeys.DocumentId, 1).Build();
				Assert.That(() => Fake.Update(doc), Throws.Exception);
			}
		}

		public sealed class TryInsert : SetupTeardownBaseBarbadosCollectionFacadeTest<TryInsert>
		{
			[Test]
			public void NoPrimaryKey_Throws()
			{
				var doc = new BarbadosDocument.Builder().Add("t", 1).Build();
				Assert.That(() => Fake.TryInsert(doc), Throws.Exception);
			}

			[Test]
			public void SamePrimaryKeyTwice_SecondInsertionFails()
			{
				var doc = new BarbadosDocument.Builder().Add(BarbadosDocumentKeys.DocumentId, 1).Build();

				var r = Fake.TryInsert(doc);
				Assert.That(r, Is.True, "First insertion failed");
				r = Fake.TryInsert(doc);
				Assert.That(r, Is.False, "Second insertion succeeded");
			}

			[Test]
			public void Many_FindAll_ReadEachDocumentBack([Values(_smallCount, _largeCount)] int count, [Values] bool hasIndex)
			{
				var rand = new XorShiftStar32(12345);
				var indexField = "index-field";

				if (hasIndex)
				{
					StubIndexAdd(indexField);
				}

				var builders = _createBuilders(count, indexField);
				foreach (var (index, builder) in builders.OrderBy(e => rand.Next()).Index())
				{
					var doc = builder.Add(BarbadosDocumentKeys.DocumentId, index + 1).Build();
					var r = Fake.TryInsert(doc);
					Assert.That(r, Is.True, "Unable to insert a document");
				}

				using var cursor = Fake.Find(FindOptions.All);
				Assert.That(cursor.Count(), Is.EqualTo(count), "Not all documents have been found in the collection");

				if (hasIndex)
				{
					using var indexCursor = Fake.Find(FindOptions.All, indexField);
					Assert.That(indexCursor.Count(), Is.EqualTo(count), "Not all documents have been found in the index");
				}
			}
		}

		public sealed class TryRemove : SetupTeardownBaseBarbadosCollectionFacadeTest<TryRemove>
		{
			[Test]
			public void NoPrimaryKey_Throws()
			{
				var doc = new BarbadosDocument.Builder().Add("t", 1).Build();
				Assert.That(() => Fake.TryRemove(doc), Throws.Exception);
			}

			[Test]
			public void SamePrimaryKeyTwice_SecondRemovalFails()
			{
				var doc = new BarbadosDocument.Builder().Add(BarbadosDocumentKeys.DocumentId, 1).Build();

				var r = Fake.TryInsert(doc);
				Assert.That(r, Is.True, "First insertion failed");
				r = Fake.TryRemove(doc);
				Assert.That(r, Is.True, "First removal failed");
				r = Fake.TryRemove(doc);
				Assert.That(r, Is.False, "Second removal succeeded");
			}

			[Test]
			public void Many_FindAll_ZeroDocumentsFound([Values(_smallCount, _largeCount)] int count, [Values] bool hasIndex)
			{
				var rand = new XorShiftStar32(12346);
				var indexField = "index-field";
				if (hasIndex)
				{
					StubIndexAdd(indexField);
				}

				var builders = _createBuilders(count);
				var docs = new BarbadosDocument[count];
				foreach (var (index, builder) in builders.Index())
				{
					docs[index] = Fake.InsertWithAutomaticId(builder);
				}

				foreach (var doc in docs.OrderBy(e => rand.Next()))
				{
					var r = Fake.TryRemove(doc);
					Assert.That(r, Is.True, "Inserted document was not removed");
				}

				using var cursor = Fake.Find(FindOptions.All);
				Assert.That(cursor.Count(), Is.Zero, "Not all documents have been removed from the collection");

				if (hasIndex)
				{
					using var indexCursor = Fake.Find(FindOptions.All, indexField);
					Assert.That(indexCursor.Count(), Is.Zero, "Not all documents have been removed from the index");
				}
			}
		}

		public sealed class TryUpdate : SetupTeardownBaseBarbadosCollectionFacadeTest<TryUpdate>
		{
			[Test]
			public void NoPrimaryKey_Throws()
			{
				var doc = new BarbadosDocument.Builder().Add("t", 1).Build();
				Assert.That(() => Fake.TryUpdate(doc), Throws.Exception);
			}

			[Test]
			public void NoDocumentWithGivenPrimaryKey_UpdateFails()
			{
				var doc = new BarbadosDocument.Builder().Add(BarbadosDocumentKeys.DocumentId, 1).Build();
				var r = Fake.TryUpdate(doc);
				Assert.That(r, Is.False, "Update succeeded for a document without a primary key");
			}

			[Test]
			public void Many_FindAll_EachDocumentFound([Values(_smallCount, _largeCount)] int count, [Values] bool hasIndex)
			{
				var rand = new XorShiftStar32(12347);
				var indexField = "index-field";
				if (hasIndex)
				{
					StubIndexAdd(indexField);
				}

				var builders = _createBuilders(count, indexField);
				var docs = new BarbadosDocument[count];
				foreach (var (index, builder) in builders.Index())
				{
					docs[index] = Fake.InsertWithAutomaticId(builder);
				}

				foreach (var doc in docs.OrderBy(e => rand.Next()))
				{
					var r = Fake.TryUpdate(doc);
					Assert.That(r, Is.True, "Inserted document was not updated");
				}

				using var cursor = Fake.Find(FindOptions.All);
				Assert.That(cursor.Count(), Is.EqualTo(count), "Not all documents have been found in the collection");
				if (hasIndex)
				{
					using var indexCursor = Fake.Find(FindOptions.All, indexField);
					Assert.That(indexCursor.Count(), Is.EqualTo(count), "Not all documents have been found in the index");
				}
			}
		}
	}
}
