using System.Collections.Generic;
using System.Linq;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	public sealed partial class BarbadosCollectionFacadeTest
	{
		/* The tests here cover the collection and an underlying clustered index.
		 * Similarly to 'BTreeIndexTest', we care about whether a collection can
		 * store and retrieve documents of different lengths
		 */

		public sealed partial class Insert : IClassFixture<BarbadosContextFixture<Insert>>
		{
			[Theory]
			[ClassData(typeof(BarbadosCollectionFacadeTestSequenceProvider))]
			public void InsertSequence_ReadEverythingBackSuccess(BarbadosCollectionFacadeTestSequence sequence)
			{
				var name = $"{nameof(InsertSequence_ReadEverythingBackSuccess)}-{sequence.Name}";
				var collection = _fixture.CreateTestBarbadosCollectionFacade(name);

				var inserted = new List<(ObjectId id, BarbadosDocument doc)>();
				foreach (var doc in sequence.Documents)
				{
					var id = collection.Insert(doc);
					inserted.Add((id, doc));
				}

				foreach (var (id, doc) in inserted)
				{
					var read = collection.TryRead(id, out var storedDocument);
					Assert.True(read);
					Assert.Equal(id, storedDocument.Id);
					Assert.Equal(doc.Buffer.Count(), storedDocument.Buffer.Count());
				}
			}
		}

		public sealed partial class TryRemove : IClassFixture<BarbadosContextFixture<TryRemove>>
		{
			[Theory]
			[ClassData(typeof(BarbadosCollectionFacadeTestSequenceProvider))]
			public void InsertSequenceThenRemoveEverything_EverythingRemovedSuccess(BarbadosCollectionFacadeTestSequence sequence)
			{
				var name = $"{nameof(InsertSequenceThenRemoveEverything_EverythingRemovedSuccess)}-{sequence.Name}";
				var collection = _fixture.CreateTestBarbadosCollectionFacade(name);

				var inserted = new List<(ObjectId id, BarbadosDocument doc)>();
				foreach (var doc in sequence.Documents)
				{
					var id = collection.Insert(doc);
				}

				foreach (var (id, doc) in inserted)
				{
					var r = collection.TryRemove(id);
					Assert.True(r);
				}

				foreach (var (id, doc) in inserted)
				{
					var r = collection.TryRead(id, out _);
					Assert.True(r);
				}
			}
		}

		public sealed partial class TryUpdate : IClassFixture<BarbadosContextFixture<TryUpdate>>
		{
			[Theory]
			[ClassData(typeof(BarbadosCollectionFacadeTestSequenceProvider))]
			public void InsertSequenceThenShuffleUpdate_ReadEverythingBackSuccess(BarbadosCollectionFacadeTestSequence sequence)
			{
				var rand = new XorShiftStar32(12345);
				var name = $"{nameof(InsertSequenceThenShuffleUpdate_ReadEverythingBackSuccess)}-{sequence.Name}";
				var collection = _fixture.CreateTestBarbadosCollectionFacade(name);

				var inserted = new List<(ObjectId id, BarbadosDocument doc)>();
				foreach (var doc in sequence.Documents)
				{
					var id = collection.Insert(doc);
					inserted.Add((id, doc));
				}

				/* Ids assigned to documents upon insertion are monotonic. In order to achieve randomness
				 * in the write pattern, we randomise the sequence of ids we update. To ensure the update
				 * doesn't write the updated document back to its original location, we take the documents
				 * belonging to different ids and use them as an updated version
				 */

				var shuffledIndices = Enumerable.Range(0, inserted.Count).OrderBy(e => rand.Next()).ToArray();
				for (var i = 0; i < shuffledIndices.Length; ++i)
				{
					var randIndex = shuffledIndices[i];
					var r = collection.TryUpdate(inserted[randIndex].id, inserted[i].doc);
					Assert.True(r);
				}

				for (var i = 0; i < shuffledIndices.Length; ++i)
				{
					var randIndex = shuffledIndices[i];
					var r = collection.TryRead(inserted[randIndex].id, out var storedDocument);
					Assert.True(r);
					Assert.Equal(inserted[randIndex].id, storedDocument.Id);
					Assert.Equal(inserted[i].doc.Count(), storedDocument.Count());
				}
			}
		}
	}
}
