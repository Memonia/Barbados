using System.Collections.Generic;
using System.Linq;

using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration.BTree
{
	internal partial class BTreeContextTest
	{
		public partial class EnumeratorTest
		{
			private static KeyValuePair<BTreeNormalisedValue, byte[]> _getKV(int value, int keyLength, int dataLength)
			{
				var key = BTreeContextTestUtils.CreateStringKeyFrom(value, keyLength);
				var data = BTreeContextTestUtils.CreateDataBytes(value, dataLength);
				return new KeyValuePair<BTreeNormalisedValue, byte[]>(key, data);
			}

			private static IEnumerable<KeyValuePair<BTreeNormalisedValue, byte[]>> _enumerateFixedKV(
				int initialValue, int count, int keyLength, int dataLength
			)
			{
				return Enumerable.Range(initialValue, count).Select(e => _getKV(e, keyLength, dataLength));
			}

			private static IEnumerable<KeyValuePair<BTreeNormalisedValue, byte[]>> _enumerateFixedKVRandomOrder(
				int initialValue, int count, int keyLength, int dataLength, int seed
			)
			{
				var rand = new XorShiftStar32(seed);
				return _enumerateFixedKV(initialValue, count, keyLength, dataLength).OrderBy(e => rand.Next());
			}
		}
	}
}
