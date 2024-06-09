using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public partial class BTreeIndexTest
	{
		public sealed class Find : IClassFixture<BarbadosContextFixture<Find>>
		{
			static Find()
			{
				// 'MixedKeys' tests depend on that
				Debug.Assert(ValueTypeMarker.Int16 < ValueTypeMarker.Int32);
				Debug.Assert(ValueTypeMarker.Int32 < ValueTypeMarker.Int64);
			}

			private readonly BarbadosContextFixture<Find> _fixture;

			public Find(BarbadosContextFixture<Find> fixture)
			{
				_fixture = fixture;
			}

			[Fact]
			public void Exact_FoundNone()
			{
				var index = _fixture.CreateTestIndex(nameof(Exact_FoundNone));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);

				var (key, keyId) = (4, new ObjectId(4));
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.Empty(result);
			}

			[Fact]
			public void Exact_FoundExact()
			{
				var index = _fixture.CreateTestIndex(nameof(Exact_FoundExact));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);

				var (key, keyId) = (v2, id2);
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.Contains(keyId, result);
				Assert.Single(result);
			}

			[Fact]
			public void Exact_FoundExactSeveral()
			{
				var index = _fixture.CreateTestIndex(nameof(Exact_FoundExactSeveral));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 2);
				var (id4, v4) = (new ObjectId(4), 2);
				var (id5, v5) = (new ObjectId(5), 3);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);

				var (key, keyIds) = (v2, new ObjectId[] { id2, id3, id4 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.Exact, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void LessThan_FoundLess()
			{
				var index = _fixture.CreateTestIndex(nameof(LessThan_FoundLess));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = (v3, new ObjectId[] { id1, id2 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.LessThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}
		
			[Fact]
			public void GreaterThan_FoundGreater()
			{
				var index = _fixture.CreateTestIndex(nameof(GreaterThan_FoundGreater));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = (v3, new ObjectId[] { id4, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.GreaterThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}
		
			[Fact]
			public void Between_FoundBetween()
			{
				var index = _fixture.CreateTestIndex(nameof(Between_FoundBetween));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (start, end, keyIds) = (v1, v5, new ObjectId[] { id2, id3, id4 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.GreaterThan, start)
					.Add(BarbadosIdentifiers.Index.LessThan, end)
					.Add(BarbadosIdentifiers.Index.Range, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void LessThanInclusive_FoundLessOrEqual()
			{
				var index = _fixture.CreateTestIndex(nameof(LessThanInclusive_FoundLessOrEqual));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = (v3, new ObjectId[] { id1, id2, id3 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.LessThan, true)
					.Add(BarbadosIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void GreaterThanInclusive_FoundGreaterOrEqual()
			{
				var index = _fixture.CreateTestIndex(nameof(GreaterThanInclusive_FoundGreaterOrEqual));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = (v3, new ObjectId[] { id3, id4, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.GreaterThan, true)
					.Add(BarbadosIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}
			
			[Fact]
			public void BetweenInclusive_FoundBetweenOrEqual()
			{
				var index = _fixture.CreateTestIndex(nameof(BetweenInclusive_FoundBetweenOrEqual));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (start, end, keyIds) = (v2, v4, new ObjectId[] { id2, id3, id4 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.GreaterThan, start)
					.Add(BarbadosIdentifiers.Index.LessThan, end)
					.Add(BarbadosIdentifiers.Index.Range, true)
					.Add(BarbadosIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void DifferentTypes_LessThan_FoundLessCorrectType()
			{
				var index = _fixture.CreateTestIndex(
					nameof(DifferentTypes_LessThan_FoundLessCorrectType)
				);

				var (id1, v1) = (new ObjectId(1), (int)1);
				var (id2, v2) = (new ObjectId(2), (short)2);
				var (id3, v3) = (new ObjectId(3), (int)3);
				var (id4, v4) = (new ObjectId(4), (short)4);
				var (id5, v5) = (new ObjectId(5), (int)5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = (v5, new ObjectId[] { id1, id3 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.LessThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void DifferentTypes_GreaterThan_FoundGreaterCorrectType()
			{
				var index = _fixture.CreateTestIndex(
					nameof(DifferentTypes_GreaterThan_FoundGreaterCorrectType)
				);

				var (id1, v1) = (new ObjectId(1), (int)1);
				var (id2, v2) = (new ObjectId(2), (long)2);
				var (id3, v3) = (new ObjectId(3), (int)3);
				var (id4, v4) = (new ObjectId(4), (long)4);
				var (id5, v5) = (new ObjectId(5), (int)5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = (v1, new ObjectId[] { id3, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.GreaterThan, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void DifferentTypes_LessThanInclusive_FoundLessOrEqualCorrectType()
			{
				var index = _fixture.CreateTestIndex(
					nameof(DifferentTypes_LessThanInclusive_FoundLessOrEqualCorrectType)
				);

				var (id1, v1) = (new ObjectId(1), (int)1);
				var (id2, v2) = (new ObjectId(2), (short)2);
				var (id3, v3) = (new ObjectId(3), (int)3);
				var (id4, v4) = (new ObjectId(4), (short)4);
				var (id5, v5) = (new ObjectId(5), (int)5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = ((int)v4, new ObjectId[] { id1, id3 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.LessThan, true)
					.Add(BarbadosIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void DifferentTypes_GreaterThanInclusive_FoundGreaterOrEqualCorrectType()
			{
				var index = _fixture.CreateTestIndex(
					nameof(DifferentTypes_GreaterThanInclusive_FoundGreaterOrEqualCorrectType)
				);

				var (id1, v1) = (new ObjectId(1), (int)1);
				var (id2, v2) = (new ObjectId(2), (long)2);
				var (id3, v3) = (new ObjectId(3), (int)3);
				var (id4, v4) = (new ObjectId(4), (long)4);
				var (id5, v5) = (new ObjectId(5), (int)5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = ((int)v2, new ObjectId[] { id3, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.GreaterThan, true)
					.Add(BarbadosIdentifiers.Index.Inclusive, true)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Take_LessThan_FoundLessThanLimited()
			{
				var index = _fixture.CreateTestIndex(nameof(Take_LessThan_FoundLessThanLimited));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = (v3, new ObjectId[] { id1, id2 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.LessThan, true)
					.Add(BarbadosIdentifiers.Index.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Take_GreaterThan_FoundGreaterThanLimited()
			{
				var index = _fixture.CreateTestIndex(nameof(Take_GreaterThan_FoundGreaterThanLimited));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (key, keyIds) = (v3, new ObjectId[] { id4, id5 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.SearchValue, key)
					.Add(BarbadosIdentifiers.Index.GreaterThan, true)
					.Add(BarbadosIdentifiers.Index.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Take_Between_FoundBetweenLimited()
			{
				var index = _fixture.CreateTestIndex(nameof(Take_Between_FoundBetweenLimited));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (start, end, keyIds) = (v1, v5, new ObjectId[] { id2, id3 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.GreaterThan, start)
					.Add(BarbadosIdentifiers.Index.LessThan, end)
					.Add(BarbadosIdentifiers.Index.Range, true)
					.Add(BarbadosIdentifiers.Index.Take, (long)2)
					.Build();

				var result = index.Find(condition).ToList();

				Assert.All(keyIds, id => Assert.Contains(id, result));
				Assert.Equal(keyIds.Length, result.Count);
			}

			[Fact]
			public void Ascending_Between_FoundBetweenInAscendingOrder()
			{
				var index = _fixture.CreateTestIndex(nameof(Ascending_Between_FoundBetweenInAscendingOrder));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (start, end, keyIds) = (v1, v5, new ObjectId[] { id2, id3, id4 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.GreaterThan, start)
					.Add(BarbadosIdentifiers.Index.LessThan, end)
					.Add(BarbadosIdentifiers.Index.Range, true)
					.Add(BarbadosIdentifiers.Index.Ascending, true)
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
				var index = _fixture.CreateTestIndex(nameof(Descending_Between_FoundBetweenInDescendingOrder));

				var (id1, v1) = (new ObjectId(1), 1);
				var (id2, v2) = (new ObjectId(2), 2);
				var (id3, v3) = (new ObjectId(3), 3);
				var (id4, v4) = (new ObjectId(4), 4);
				var (id5, v5) = (new ObjectId(5), 5);
				index.Insert(NormalisedValue.Create(v1), id1);
				index.Insert(NormalisedValue.Create(v2), id2);
				index.Insert(NormalisedValue.Create(v3), id3);
				index.Insert(NormalisedValue.Create(v4), id4);
				index.Insert(NormalisedValue.Create(v5), id5);

				var (start, end, keyIds) = (v1, v5, new ObjectId[] { id4, id3, id2 });
				var condition = new BarbadosDocument.Builder()
					.Add(BarbadosIdentifiers.Index.GreaterThan, start)
					.Add(BarbadosIdentifiers.Index.LessThan, end)
					.Add(BarbadosIdentifiers.Index.Range, true)
					.Add(BarbadosIdentifiers.Index.Ascending, false)
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
