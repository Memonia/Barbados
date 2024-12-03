using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Documents.Serialisation.Values;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public partial class BTreeIndexFacadeTest
	{
		public sealed partial class Find : IClassFixture<BarbadosContextFixture<Find>>
		{
			private static readonly string _field = "test";

			static Find()
			{
				// 'MixedKeys' tests depend on that
				Debug.Assert(ValueTypeMarker.Int16 < ValueTypeMarker.Int32);
				Debug.Assert(ValueTypeMarker.Int32 < ValueTypeMarker.Int64);
			}

			[Fact]
			public void Exact_FoundNone()
			{
				var name = nameof(Exact_FoundNone);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);

				var (key, keyId) = (4, new ObjectId(4));
				var condition = new BarbadosDocument.Builder()
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.Empty(result);
			}

			[Fact]
			public void Exact_FoundExact()
			{
				var name = nameof(Exact_FoundExact);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
				var builder = new BarbadosDocument.Builder();

				var v1 = builder.Add(_field, 1).Build();
				var v2 = builder.Add(_field, 2).Build();
				var v3 = builder.Add(_field, 3).Build();
				var id1 = collection.Insert(v1);
				var id2 = collection.Insert(v2);
				var id3 = collection.Insert(v3);

				var (key, keyId) = (2, id2);
				var condition = new BarbadosDocument.Builder()
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.Single(result);
				Assert.Equal(keyId, result[0]);
			}

			[Fact]
			public void Exact_FoundExactSeveral()
			{
				var name = nameof(Exact_FoundExactSeveral);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void LessThan_FoundLess()
			{
				var name = nameof(LessThan_FoundLess);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.LessThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void GreaterThan_FoundGreater()
			{
				var name = nameof(GreaterThan_FoundGreater);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.GreaterThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Between_FoundBetween()
			{
				var name = nameof(Between_FoundBetween);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.GreaterThan, start)
					.Add(CommonIdentifiers.Index.LessThan, end)
					.Add(CommonIdentifiers.Index.Range, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void LessThanInclusive_FoundLessOrEqual()
			{
				var name = nameof(LessThanInclusive_FoundLessOrEqual);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.LessThan, true)
					.Add(CommonIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void GreaterThanInclusive_FoundGreaterOrEqual()
			{
				var name = nameof(GreaterThanInclusive_FoundGreaterOrEqual);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.GreaterThan, true)
					.Add(CommonIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void BetweenInclusive_FoundBetweenOrEqual()
			{
				var name = nameof(BetweenInclusive_FoundBetweenOrEqual);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.GreaterThan, start)
					.Add(CommonIdentifiers.Index.LessThan, end)
					.Add(CommonIdentifiers.Index.Range, true)
					.Add(CommonIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void DifferentTypes_LessThan_FoundLessCorrectType()
			{
				var name = nameof(DifferentTypes_LessThan_FoundLessCorrectType);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.LessThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void DifferentTypes_GreaterThan_FoundGreaterCorrectType()
			{
				var name = nameof(DifferentTypes_GreaterThan_FoundGreaterCorrectType);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.GreaterThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void DifferentTypes_LessThanInclusive_FoundLessOrEqualCorrectType()
			{
				var name = nameof(DifferentTypes_LessThanInclusive_FoundLessOrEqualCorrectType);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.LessThan, true)
					.Add(CommonIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void DifferentTypes_GreaterThanInclusive_FoundGreaterOrEqualCorrectType()
			{
				var name = nameof(DifferentTypes_GreaterThanInclusive_FoundGreaterOrEqualCorrectType);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.GreaterThan, true)
					.Add(CommonIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Take_LessThan_FoundLessThanLimited()
			{
				var name = nameof(Take_LessThan_FoundLessThanLimited);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.LessThan, true)
					.Add(CommonIdentifiers.Index.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Take_GreaterThan_FoundGreaterThanLimited()
			{
				var name = nameof(Take_GreaterThan_FoundGreaterThanLimited);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.SearchValue, key)
					.Add(CommonIdentifiers.Index.GreaterThan, true)
					.Add(CommonIdentifiers.Index.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Take_Between_FoundBetweenLimited()
			{
				var name = nameof(Take_Between_FoundBetweenLimited);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.GreaterThan, start)
					.Add(CommonIdentifiers.Index.LessThan, end)
					.Add(CommonIdentifiers.Index.Range, true)
					.Add(CommonIdentifiers.Index.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Ascending_Between_FoundBetweenInAscendingOrder()
			{
				var name = nameof(Ascending_Between_FoundBetweenInAscendingOrder);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.GreaterThan, start)
					.Add(CommonIdentifiers.Index.LessThan, end)
					.Add(CommonIdentifiers.Index.Range, true)
					.Add(CommonIdentifiers.Index.Ascending, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.Equal(keyIds.Length, result.Count);
				Assert.Equal(keyIds[0], result[0]);
				Assert.Equal(keyIds[1], result[1]);
				Assert.Equal(keyIds[2], result[2]);
			}

			[Fact]
			public void Descending_Between_FoundBetweenInDescendingOrder()
			{
				var name = nameof(Descending_Between_FoundBetweenInDescendingOrder);
				var index = _fixture.CreateTestBTreeIndexFacade(name);
				var collection = _fixture.GetTestBarbadosCollectionFacade(name);
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
					.Add(CommonIdentifiers.Index.GreaterThan, start)
					.Add(CommonIdentifiers.Index.LessThan, end)
					.Add(CommonIdentifiers.Index.Range, true)
					.Add(CommonIdentifiers.Index.Ascending, false)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.Equal(keyIds.Length, result.Count);
				Assert.Equal(keyIds[0], result[2]);
				Assert.Equal(keyIds[1], result[1]);
				Assert.Equal(keyIds[2], result[0]);
			}
		}
	}
}
