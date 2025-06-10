using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Tests.Integration.BTree
{
	internal partial class BTreeContextTest
	{
		public sealed partial class EnumeratorTest : SetupTeardownBTreeContextTest<EnumeratorTest>
		{
			/* Notes:
			 * 
			 * 1. Entries are inserted in random order to ensure the order of operations doesn't affect correctness of enumeration
			 * 2. For now it doesn't matter whether only the key overflows, the data or both - both would be written as a chunk sequence
			 * if the combined length exceeds maximum allowed in a tree. This is why tests only have 'bool overflow' as a parameter. When 
			 * this behavior changes, tests must be changed accordingly
			 */

			private const int _inlineKeyLength = BTreeInfo.LimitMaxExternalLookupKeyLength - 64;
			private const int _inlineDataLength = BTreeInfo.LimitMaxDataLength - 64;
			private const int _overflowKeyLength = BTreeInfo.LimitMaxExternalLookupKeyLength + 64;
			private const int _overflowDataLength = BTreeInfo.LimitMaxDataLength + 64;

			private const int _minCountForPageSplit = Constants.PageLength / (_inlineKeyLength + _inlineDataLength);

			// A number which causes many splits, but also keeps the duration of the tests in reasonable bounds
			private const int _defaultCountForPageSplits = _minCountForPageSplit * 32;

			[Test]
			[TestCase(1)]
			[TestCase(2)]
			public void InsertNone_RetrievalFails(int count)
			{
				var enumerator = new BTreeContext.Enumerator(Context, BTreeFindOptions.FindAll);
				for (var i = 0; i < count; ++i)
				{
					var a = enumerator.MoveNext();
					var b = enumerator.TryGetCurrentKey(out _);
					var c = enumerator.TryGetCurrentData(out _);
					var d = enumerator.TryGetCurrentKeyAsSpan(out _);
					var e = enumerator.TryGetCurrentDataAsSpan(out _);

					Assert.Multiple(() =>
					{
						Assert.That(a, Is.False, "MoveNext incorrect return value");
						Assert.That(b, Is.False, $"{nameof(enumerator.TryGetCurrentKey)} incorrect return value");
						Assert.That(c, Is.False, $"{nameof(enumerator.TryGetCurrentData)} incorrect return value");
						Assert.That(d, Is.False, $"{nameof(enumerator.TryGetCurrentKeyAsSpan)} incorrect return value");
						Assert.That(e, Is.False, $"{nameof(enumerator.TryGetCurrentDataAsSpan)} incorrect return value");
					});
				}
			}

			[Test, Combinatorial]
			public void InsertMany_Find_All_CorrectEnumeration(
				[Values(8, _defaultCountForPageSplits)] int count,
				[Values] bool keyOverflows,
				[Values] bool dataOverflows
			)
			{
				var seed = 12345;
				var initialValue = 0;
				var keyLength = keyOverflows ? _overflowKeyLength : _inlineKeyLength;
				var dataLength = dataOverflows ? _overflowDataLength : _inlineDataLength;
				foreach (var (k, d) in _enumerateFixedKVRandomOrder(
					initialValue: initialValue, count: count, keyLength: keyLength, dataLength: dataLength, seed: seed
					)
				)
				{
					var r = Context.TryInsert(k, d);
					Assert.That(r, Is.True, "Insertion failed");
				}

				var enumerator = new BTreeContext.Enumerator(Context, BTreeFindOptions.FindAll);
				var verifier = new Verifier(enumerator);
				foreach (var (k, d) in _enumerateFixedKV(
					initialValue: initialValue, count: count, keyLength: keyLength, dataLength: dataLength
					)
				)
				{
					verifier.AssertMoveNext(true);
					verifier.AssertTryGetRetrieval(k.AsSpan().Bytes, d);
				}

				verifier.AssertMoveNext(false);
			}

			[Test, Combinatorial]
			public void InsertMany_Find_Reverse_CorrectEnumeration(
				[Values(8, _defaultCountForPageSplits)] int count,
				[Values] bool reverse,
				[Values] bool keyOverflows,
				[Values] bool dataOverflows
			)
			{
				var seed = 12346;
				var initialValue = 0;
				var keyLength = keyOverflows ? _overflowKeyLength : _inlineKeyLength;
				var dataLength = dataOverflows ? _overflowDataLength : _inlineDataLength;
				foreach (var (k, d) in _enumerateFixedKVRandomOrder(
					initialValue: initialValue, count: count, keyLength: keyLength, dataLength: dataLength, seed: seed
					)
				)
				{
					var r = Context.TryInsert(k, d);
					Assert.That(r, Is.True, "Insertion failed");
				}

				var options = BTreeFindOptions.FindAll with { Reverse = reverse };
				var enumerator = new BTreeContext.Enumerator(Context, options);
				var verifier = new Verifier(enumerator);
				for (var i = 0; i < count; ++i)
				{
					var value = reverse ? count - i - 1 : i;
					var (k, d) = _getKV(value, keyLength, dataLength);

					verifier.AssertMoveNext(true);
					verifier.AssertTryGetRetrieval(k.AsSpan().Bytes, d);
				}

				verifier.AssertMoveNext(false);
			}

			[Test, Combinatorial]
			public void InsertMany_Find_Limit_CorrectEnumeration(
				[Values(8, _defaultCountForPageSplits)] int count,
				[Values(0, 4, _defaultCountForPageSplits - 1)] int limit,
				[Values] bool keyOverflows,
				[Values] bool dataOverflows
			)
			{
				var seed = 12347;
				var initialValue = 0;
				var keyLength = keyOverflows ? _overflowKeyLength : _inlineKeyLength;
				var dataLength = dataOverflows ? _overflowDataLength : _inlineDataLength;
				foreach (var (k, d) in _enumerateFixedKVRandomOrder(
					initialValue: initialValue, count: count, keyLength: keyLength, dataLength: dataLength, seed: seed
					)
				)
				{
					var r = Context.TryInsert(k, d);
					Assert.That(r, Is.True, "Insertion failed");
				}

				var options = BTreeFindOptions.FindAll with { Limit = limit };
				var enumerator = new BTreeContext.Enumerator(Context, options);
				var verifier = new Verifier(enumerator);
				for (var i = 0; i < count; ++i)
				{
					if (i < limit)
					{
						var (k, d) = _getKV(i, keyLength, dataLength);

						verifier.AssertMoveNext(true);
						verifier.AssertTryGetRetrieval(k.AsSpan().Bytes, d);
					}

					else
					{
						verifier.AssertMoveNext(false);
					}
				}

				verifier.AssertMoveNext(false);
			}

			[Test, Combinatorial]
			public void InsertMany_Find_Skip_CorrectEnumeration(
				[Values(8, _defaultCountForPageSplits)] int count,
				[Values(0, 4, _defaultCountForPageSplits - 1)] int skip,
				[Values] bool keyOverflows,
				[Values] bool dataOverflows
			)
			{
				var seed = 12348;
				var initialValue = 0;
				var keyLength = keyOverflows ? _overflowKeyLength : _inlineKeyLength;
				var dataLength = dataOverflows ? _overflowDataLength : _inlineDataLength;
				foreach (var (k, d) in _enumerateFixedKVRandomOrder(
					initialValue: initialValue, count: count, keyLength: keyLength, dataLength: dataLength, seed: seed
					)
				)
				{
					var r = Context.TryInsert(k, d);
					Assert.That(r, Is.True, "Insertion failed");
				}

				var options = BTreeFindOptions.FindAll with { Skip = skip };
				var enumerator = new BTreeContext.Enumerator(Context, options);
				var verifier = new Verifier(enumerator);
				for (var i = skip; i < count; ++i)
				{
					if (i < skip)
					{
						continue;
					}

					else
					{
						var (k, d) = _getKV(i, keyLength, dataLength);

						verifier.AssertMoveNext(true);
						verifier.AssertTryGetRetrieval(k.AsSpan().Bytes, d);
					}
				}

				verifier.AssertMoveNext(false);
			}

			[Test, Combinatorial]
			public void InsertMany_Find_SkipLimitReverse_CorrectEnumeration(
				[Values(_defaultCountForPageSplits)] int count,
				[Values(16, _defaultCountForPageSplits - 1)] int skip,
				[Values(16, _defaultCountForPageSplits - 1)] int limit,
				[Values] bool reverse,
				[Values] bool keyOverflows,
				[Values] bool dataOverflows
			)
			{
				var seed = 12349;
				var initialValue = 0;
				var keyLength = keyOverflows ? _overflowKeyLength : _inlineKeyLength;
				var dataLength = dataOverflows ? _overflowDataLength : _inlineDataLength;
				foreach (var (k, d) in _enumerateFixedKVRandomOrder(
					initialValue: initialValue, count: count, keyLength: keyLength, dataLength: dataLength, seed: seed
					)
				)
				{
					var r = Context.TryInsert(k, d);
					Assert.That(r, Is.True, "Insertion failed");
				}

				var options = BTreeFindOptions.FindAll with { Skip = skip, Limit = limit, Reverse = reverse };
				var enumerator = new BTreeContext.Enumerator(Context, options);
				var verifier = new Verifier(enumerator);
				for (int i = 0; i < count; ++i)
				{
					if (i < skip)
					{
						continue;
					}

					if (i - skip < limit)
					{
						var value = reverse ? count - i - 1 : i;
						var (k, d) = _getKV(value, keyLength, dataLength);

						verifier.AssertMoveNext(true);
						verifier.AssertTryGetRetrieval(k.AsSpan().Bytes, d);
					}

					else
					{
						verifier.AssertMoveNext(false);
					}
				}

				verifier.AssertMoveNext(false);
			}
		}
	}
}
