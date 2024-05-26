using System.Diagnostics;

using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public sealed partial class BTreeIndexTest
	{
		public sealed class Insert : IClassFixture<BarbadosContextFixture<Insert>>
		{
			private readonly BarbadosContextFixture<Insert> _fixture;

			public Insert(BarbadosContextFixture<Insert> fixture)
			{
				_fixture = fixture;
			}

			[Theory]
			[ClassData(typeof(BTreeIndexTestSequenceProvider))]
			public void InsertSequence_ReadEverythingBackSuccess(BTreeIndexTestSequence sequence)
			{
				var name = $"{nameof(InsertSequence_ReadEverythingBackSuccess)}-{sequence.DocumentSequence.Name}";
				var index = _fixture.CreateTestIndex(name, sequence);
				var collection = _fixture.Context.Controller.GetCollection(name);

				var ids = new Dictionary<object, List<ObjectId>>();
				foreach (var document in sequence.DocumentSequence.Documents)
				{
					var r = document.TryGet(sequence.IndexedField, out var key);
					Debug.Assert(r);

					var id = collection.Insert(document);
					if (ids.TryGetValue(key, out var existingIds))
					{
						existingIds.Add(id);
					}

					else
					{
						ids.Add(key, [id]);
					}
				}

				foreach (var (key, expectedIds) in ids)
				{
					var foundIds = index.FindExact(key).ToList();
					Assert.Equal(expectedIds.Count, foundIds.Count);
					Assert.All(
						expectedIds, e => Assert.Contains(e, foundIds)
					);
				}
			}
		}

		public sealed class TryRemove : IClassFixture<BarbadosContextFixture<TryRemove>>
		{
			private readonly BarbadosContextFixture<TryRemove> _fixture;

			public TryRemove(BarbadosContextFixture<TryRemove> fixture)
			{
				_fixture = fixture;
			}

			[Theory]
			[ClassData(typeof(BTreeIndexTestSequenceProvider))]
			public void InsertSequenceThenRemoveAll_EverythingRemovedSuccess(BTreeIndexTestSequence sequence)
			{
				var name = $"{nameof(InsertSequenceThenRemoveAll_EverythingRemovedSuccess)}-{sequence.DocumentSequence.Name}";
				var index = _fixture.CreateTestIndex(name, sequence);
				var collection = _fixture.Context.Controller.GetCollection(name);

				var ids = new Dictionary<object, List<ObjectId>>();
				foreach (var document in sequence.DocumentSequence.Documents)
				{
					var r = document.TryGet(sequence.IndexedField, out var key);
					Debug.Assert(r);

					var id = collection.Insert(document);
					if (ids.TryGetValue(key, out var existingIds))
					{
						existingIds.Add(id);
					}

					else
					{
						ids.Add(key, [id]);
					}
				}

				foreach (var (key, idList) in ids)
				{
					foreach (var id in idList)
					{
						var r = collection.TryRemove(id);
						Assert.True(r);
					}
				}

				foreach (var (key, __REMOVE_ID) in ids)
				{
					var foundIds = index.FindExact(key).ToList();
					Assert.Empty(foundIds);
				}
			}
		}
	}
}
