using System;

using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Tests.Integration.BTree
{
	internal sealed partial class BTreeContextTest
	{
		public sealed class Deallocate : SetupTeardownBTreeContextTest<Deallocate>
		{
			[Test]
			public void InsertData_Deallocate_NoErrors()
			{
				var count = 16;
				for (var i = 0; i < count; ++i)
				{
					var key = BTreeContextTestUtils.CreateStringKeyFrom(i, Constants.PageLength);
					var data = new byte[] { (byte)i };
					var r = Context.TryInsert(key, data);
					Assert.That(r, Is.True, "Insertion failed");
				}

				Context.Deallocate();
			}
		}

		public sealed class TryInsert : SetupTeardownBTreeContextTest<TryInsert>
		{
			[Test]
			public void SameKeyTwice_SecondInsertionFails()
			{
				var key = BTreeNormalisedValue.Create("test", isKeyExternal: false);
				var data = new byte[] { 1, 2, 3, 4, 5 };

				var r = Context.TryInsert(key, data);
				Assert.That(r, Is.True, "First insertion failed");

				r = Context.TryInsert(key, data);
				Assert.That(r, Is.False, "Second insertion succeeded");
			}

			[Test]
			[TestCaseSource(typeof(BTreeContextTestSequenceProvider))]
			public void Sequence_TryFind_EachKeyDataPairFound(BTreeContextTestSequence sequence)
			{
				foreach (var (key, data) in sequence.KeyDataPairs)
				{
					var r = Context.TryInsert(key, data);
					Assert.That(r, Is.True, "Test sequence contains duplicate keys");
				}

				foreach (var (key, data) in sequence.KeyDataPairs)
				{
					var r = Context.TryFind(key, out var outData);
					Assert.Multiple(() =>
					{
						Assert.That(r, Is.True, "Inserted key could not be found");
						Assert.That(outData.AsSpan().SequenceEqual(data), "Wrong data entry for a given key");
					});
				}
			}
		}

		public sealed class TryRemove : SetupTeardownBTreeContextTest<TryRemove>
		{
			[Test]
			public void SameKeyTwice_SecondRemovalFails()
			{
				var key = BTreeNormalisedValue.Create("test", isKeyExternal: false);
				var data = new byte[] { 1, 2, 3, 4, 5 };

				var r = Context.TryInsert(key, data);
				Assert.That(r, Is.True, "First insertion failed");

				r = Context.TryRemove(key);
				Assert.That(r, Is.True, "First removal failed");

				r = Context.TryRemove(key);
				Assert.That(r, Is.False, "Second removal succeeded");
			}

			[Test]
			[TestCaseSource(typeof(BTreeContextTestSequenceProvider))]
			public void Sequence_TryFind_ZeroKeyDataPairsFound(BTreeContextTestSequence sequence)
			{
				foreach (var (key, data) in sequence.KeyDataPairs)
				{
					var r = Context.TryInsert(key, data);
					Assert.That(r, Is.True, "Test sequence contains duplicate keys");
				}

				foreach (var (key, _) in sequence.KeyDataPairs)
				{
					var r = Context.TryRemove(key);
					Assert.That(r, Is.True, "Inserted key could not be removed");
				}

				var e = Context.GetDataEnumerator(BTreeFindOptions.FindAllIncludeExternal);
				var itemExists = e.MoveNext();
				Assert.That(itemExists, Is.False, "Some keys remain in the BTree");
			}
		}
	}
}
