using System.Diagnostics;
using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public sealed partial class BTreeIndexFacadeTest
	{
		public sealed class Find : SetupTeardownBarbadosContextTestClass<Find>
		{
			private static readonly string _field = "test";

			static Find()
			{
				// 'MixedKeys' tests depend on that
				Debug.Assert(NormalisedValueType.Int16 < NormalisedValueType.Int32);
				Debug.Assert(NormalisedValueType.Int32 < NormalisedValueType.Int64);
			}

			[Test]
			public void Exact_FoundNone()
			{
				var name = nameof(Exact_FoundNone);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);

				var (key, keyId) = (4, new ObjectId(4));
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.Empty);
			}

			[Test]
			public void Exact_FoundExact()
			{
				var name = nameof(Exact_FoundExact);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);

				var (key, keyId) = (2, id2);
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.Multiple(() =>
				{
					Assert.That(result, Has.Count.EqualTo(1));
					Assert.That(keyId, Is.EqualTo(result[0]));
				});
			}

			[Test]
			public void Exact_FoundExactSeveral()
			{
				var name = nameof(Exact_FoundExactSeveral);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 2).Build();
				var v4 = builder.Add(_field, 2).Build();
				var v5 = builder.Add(_field, 3).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (2, new ObjectId[] { id2, id3, id4 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void LessThan_FoundLess()
			{
				var name = nameof(LessThan_FoundLess);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (3, new ObjectId[] { id1, id2 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void GreaterThan_FoundGreater()
			{
				var name = nameof(GreaterThan_FoundGreater);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (3, new ObjectId[] { id4, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void Between_FoundBetween()
			{
				var name = nameof(Between_FoundBetween);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (start, end, keyIds) = (1, 5, new ObjectId[] { id2, id3, id4 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, start)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, end)
					.Add(BarbadosDocumentKeys.IndexQuery.Range, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void LessThanInclusive_FoundLessOrEqual()
			{
				var name = nameof(LessThanInclusive_FoundLessOrEqual);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (3, new ObjectId[] { id1, id2, id3 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void GreaterThanInclusive_FoundGreaterOrEqual()
			{
				var name = nameof(GreaterThanInclusive_FoundGreaterOrEqual);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (3, new ObjectId[] { id3, id4, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void BetweenInclusive_FoundBetweenOrEqual()
			{
				var name = nameof(BetweenInclusive_FoundBetweenOrEqual);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (start, end, keyIds) = (2, 4, new ObjectId[] { id2, id3, id4 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, start)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, end)
					.Add(BarbadosDocumentKeys.IndexQuery.Range, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void DifferentTypes_LessThan_FoundLessCorrectType()
			{
				var name = nameof(DifferentTypes_LessThan_FoundLessCorrectType);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, (int)1).Build();
				var v2 = builder.Add(_field, (short)2).Build();
				var v3 = builder.Add(_field, (int)3).Build();
				var v4 = builder.Add(_field, (short)4).Build();
				var v5 = builder.Add(_field, (int)5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (5, new ObjectId[] { id1, id3 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void DifferentTypes_GreaterThan_FoundGreaterCorrectType()
			{
				var name = nameof(DifferentTypes_GreaterThan_FoundGreaterCorrectType);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, (int)1).Build();
				var v2 = builder.Add(_field, (long)2).Build();
				var v3 = builder.Add(_field, (int)3).Build();
				var v4 = builder.Add(_field, (long)4).Build();
				var v5 = builder.Add(_field, (int)5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (1, new ObjectId[] { id3, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void DifferentTypes_LessThanInclusive_FoundLessOrEqualCorrectType()
			{
				var name = nameof(DifferentTypes_LessThanInclusive_FoundLessOrEqualCorrectType);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, (int)1).Build();
				var v2 = builder.Add(_field, (short)2).Build();
				var v3 = builder.Add(_field, (int)3).Build();
				var v4 = builder.Add(_field, (short)4).Build();
				var v5 = builder.Add(_field, (int)5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (4, new ObjectId[] { id1, id3 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void DifferentTypes_GreaterThanInclusive_FoundGreaterOrEqualCorrectType()
			{
				var name = nameof(DifferentTypes_GreaterThanInclusive_FoundGreaterOrEqualCorrectType);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, (int)1).Build();
				var v2 = builder.Add(_field, (long)2).Build();
				var v3 = builder.Add(_field, (int)3).Build();
				var v4 = builder.Add(_field, (long)4).Build();
				var v5 = builder.Add(_field, (int)5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (2, new ObjectId[] { id3, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void Take_LessThan_FoundLessThanLimited()
			{
				var name = nameof(Take_LessThan_FoundLessThanLimited);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (3, new ObjectId[] { id1, id2 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void Take_GreaterThan_FoundGreaterThanLimited()
			{
				var name = nameof(Take_GreaterThan_FoundGreaterThanLimited);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (key, keyIds) = (3, new ObjectId[] { id4, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.SearchValue, key)
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void Take_Between_FoundBetweenLimited()
			{
				var name = nameof(Take_Between_FoundBetweenLimited);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (start, end, keyIds) = (1, 5, new ObjectId[] { id2, id3 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, start)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, end)
					.Add(BarbadosDocumentKeys.IndexQuery.Range, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EquivalentTo(keyIds));
			}

			[Test]
			public void Ascending_Between_FoundBetweenInAscendingOrder()
			{
				var name = nameof(Ascending_Between_FoundBetweenInAscendingOrder);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (start, end, keyIds) = (1, 5, new ObjectId[] { id2, id3, id4 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, start)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, end)
					.Add(BarbadosDocumentKeys.IndexQuery.Range, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Ascending, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EqualTo(keyIds));
			}

			[Test]
			public void Descending_Between_FoundBetweenInDescendingOrder()
			{
				var name = nameof(Descending_Between_FoundBetweenInDescendingOrder);
				var index = Context.CreateTestBTreeIndexFacade(name);
				var collection = Context.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var v4 = builder.Add(_field, 4).Build();
				var v5 = builder.Add(_field, 5).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);
				var id4 = collection.Insert(v4);
				var id5 = collection.Insert(v5);

				var (start, end, keyIds) = (1, 5, new ObjectId[] { id4, id3, id2 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosDocumentKeys.IndexQuery.GreaterThan, start)
					.Add(BarbadosDocumentKeys.IndexQuery.LessThan, end)
					.Add(BarbadosDocumentKeys.IndexQuery.Range, true)
					.Add(BarbadosDocumentKeys.IndexQuery.Ascending, false)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.That(result, Is.EqualTo(keyIds.Reverse()));
			}
		}
	}
}
