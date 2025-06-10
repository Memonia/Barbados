using System;
using System.Linq;

using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Tests.Integration.BTree
{
	internal partial class BTreeContextTest
	{
		public partial class EnumeratorTest
		{
			private sealed class Verifier
			{
				private readonly BTreeContext.Enumerator _enumerator;

				public Verifier(BTreeContext.Enumerator enumerator)
				{
					_enumerator = enumerator;
				}

				public void AssertMoveNext(bool expected)
				{
					var actual = _enumerator.MoveNext();
					Assert.That(actual, Is.EqualTo(expected), "MoveNext incorrect return value");
				}

				public void AssertTryGetRetrieval(ReadOnlySpan<byte> expectedKey, ReadOnlySpan<byte> expectedData)
				{
					var kReturned = _enumerator.TryGetCurrentKey(out var k);
					var dReturned = _enumerator.TryGetCurrentData(out var d);
					var kspanReturned = _enumerator.TryGetCurrentKeyAsSpan(out var kspan);
					var dspanReturned = _enumerator.TryGetCurrentDataAsSpan(out var dspan);

					var gotKey = kReturned || kspanReturned;
					var gotData = dReturned || dspanReturned;
					Assert.Multiple(() =>
					{
						Assert.That(gotKey, Is.True, "Could not retrieve the key");
						Assert.That(gotData, Is.True, "Could not retrieve the data");
					});

					var keyMatch = kspanReturned
						? kspan.Bytes.SequenceEqual(expectedKey)
						: k.AsSpan().Bytes.SequenceEqual(expectedKey);

					var dataMatch = dspanReturned
						? dspan.SequenceEqual(expectedData)
						: d.AsSpan().SequenceEqual(expectedData);

					Assert.Multiple(() =>
					{
						Assert.That(keyMatch, Is.True, "Key bytes do not match");
						Assert.That(dataMatch, Is.True, "Data bytes do not match");
					});
				}
			}
		}
	}
}
