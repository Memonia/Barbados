using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Tests.Integration.Utils;

namespace Barbados.StorageEngine.Tests.Integration.BTree
{
#pragma warning disable IDE0305 // Collection initialisation can be simplified
	internal sealed class BTreeContextTestSequenceProvider : IEnumerable<BTreeContextTestSequence>
	{
		private static KeyValuePair<BTreeNormalisedValue, byte[]> _getKeyWithEmptyDataPair<T>(T key)
		{
			return new(BTreeNormalisedValue.Create(key, isKeyExternal: false), []);
		}

		private static KeyValuePair<BTreeNormalisedValue, byte[]> _getStrKeyWithEmptyDataPair(int key, int length)
		{
			return _getStrKeyWithDataPair(key, length, 0);
		}

		private static KeyValuePair<BTreeNormalisedValue, byte[]> _getStrKeyWithDataPair(int key, int keyLength, int dataLength)
		{
			var k = BTreeContextTestUtils.CreateStringKeyFrom(key, keyLength);
			return new(k, BTreeContextTestUtils.CreateDataBytes(key, dataLength));
		}

		private static KeyValuePair<BTreeNormalisedValue, byte[]> _getExternalStrKeyWithDataPair(int key, int keyLength, int dataLength)
		{
			var k = BTreeContextTestUtils.CreateStringKeyFrom(key, keyLength, isKeyExternal: true);
			return new(k, BTreeContextTestUtils.CreateDataBytes(key, dataLength));
		}

		public IEnumerator<BTreeContextTestSequence> GetEnumerator()
		{
			var s1 = 64;
			var s2 = BTreeInfo.LimitMaxLookupKeyLength;
			var s3 = Constants.PageLength + 1;

			var r = new XorShiftStar32(271828182);
			var seq1 = Enumerable.Range(1, s1).ToArray();
			var seq2 = Enumerable.Range(1, s2).ToArray();
			var seq3 = Enumerable.Range(1, s3).ToArray();

			// A bigger sequence for variable-length keys
			var vlseq = seq1.Concat(Enumerable.Range(s3, 100)).ToArray();

			// A smaller sequence for big fixed-length s3 keys
			var s3fseq = Enumerable.Range(1, 100).ToArray();

			// Sequences for supported key types
			//                                                                        |nice
			var tseqs = new ValueTuple<string, KeyValuePair<BTreeNormalisedValue, byte[]>[]>[]
			{
				("T_INT8",     [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((sbyte)e))]),
				("T_INT16",    [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((short)e))]),
				("T_INT32",    [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair(e))]),
				("T_INT64",    [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((long)e))]),
				("T_UINT8",    [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((byte)e))]),
				("T_UINT16",   [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((ushort)e))]),
				("T_UINT32",   [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((uint)e))]),
				("T_UINT64",   [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((ulong)e))]),
				("T_FLOAT32",  [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((float)e))]),
				("T_FLOAT64",  [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair((double)e))]),
				("T_DATETIME", [.. seq1.OrderBy(e => r.Next()).Select(e => _getKeyWithEmptyDataPair(new DateTime(e)))]),
				("T_STRING",   [.. seq1.OrderBy(e => r.Next()).Select(e => _getStrKeyWithEmptyDataPair(e, e))]),
				("T_BOOL",     [_getKeyWithEmptyDataPair(true), _getKeyWithEmptyDataPair(false)]),
				("T_EXTERNAL", [.. seq1.OrderBy(e => r.Next()).Select(e => _getExternalStrKeyWithDataPair(e, e + 1, e))])
			};

			/* Different key sequences for different insert/remove operation patterns
			 */

			yield return new("samePrefix_iKoK", [
				_getStrKeyWithEmptyDataPair(1, BTreeInfo.LimitMaxExternalLookupKeyLength),
				_getStrKeyWithEmptyDataPair(1, BTreeInfo.LimitMaxExternalLookupKeyLength + 1)
			]);

			yield return new("samePrefix_oKiK", [
				_getStrKeyWithEmptyDataPair(1, BTreeInfo.LimitMaxExternalLookupKeyLength + 1),
				_getStrKeyWithEmptyDataPair(1, BTreeInfo.LimitMaxExternalLookupKeyLength)
			]);

			// Combinations of inline/overflow keys and inline/overflow data
			yield return new("iKiD", [_getStrKeyWithDataPair(1, BTreeInfo.LimitMaxExternalLookupKeyLength - 1, BTreeInfo.LimitMaxDataLength - 1)]);
			yield return new("iKoD", [_getStrKeyWithDataPair(1, BTreeInfo.LimitMaxExternalLookupKeyLength - 1, BTreeInfo.LimitMaxDataLength + 1)]);
			yield return new("oKiD", [_getStrKeyWithDataPair(1, BTreeInfo.LimitMaxExternalLookupKeyLength + 1, BTreeInfo.LimitMaxDataLength - 1)]);
			yield return new("oKoD", [_getStrKeyWithDataPair(1, BTreeInfo.LimitMaxExternalLookupKeyLength + 1, BTreeInfo.LimitMaxDataLength + 1)]);

			// Fixed length keys: ascending, descending, unordered
			yield return new("AF1", seq1.Select(e => _getStrKeyWithEmptyDataPair(e, s1)).ToList());
			yield return new("AF2", seq2.Select(e => _getStrKeyWithEmptyDataPair(e, s2)).ToList());
			yield return new("AF3", s3fseq.Select(e => _getStrKeyWithEmptyDataPair(e, s3)).ToList());
			yield return new("DF1", seq1.Reverse().Select(e => _getStrKeyWithEmptyDataPair(e, s1)).ToList());
			yield return new("DF2", seq2.Reverse().Select(e => _getStrKeyWithEmptyDataPair(e, s2)).ToList());
			yield return new("DF3", s3fseq.Reverse().Select(e => _getStrKeyWithEmptyDataPair(e, s3)).ToList());
			yield return new("UF1", seq1.OrderBy(e => r.Next()).Select(e => _getStrKeyWithEmptyDataPair(e, s1)).ToList());
			yield return new("UF2", seq2.OrderBy(e => r.Next()).Select(e => _getStrKeyWithEmptyDataPair(e, s2)).ToList());
			yield return new("UF3", s3fseq.OrderBy(e => r.Next()).Select(e => _getStrKeyWithEmptyDataPair(e, s3)).ToList());

			// Variable length keys: ascending, descending, unordered
			yield return new("AV", vlseq.Select(e => _getStrKeyWithEmptyDataPair(e, e)).ToList());
			yield return new("DV", vlseq.Reverse().Select(e => _getStrKeyWithEmptyDataPair(e, e)).ToList());
			yield return new("UV", vlseq.OrderBy(e => r.Next()).Select(e => _getStrKeyWithEmptyDataPair(e, e)).ToList());

			// Different patterns of insert/remove operations with variable length data entries
			yield return new("AF3dv", s3fseq.Select(e => _getStrKeyWithDataPair(e, s3, e)).ToList());
			yield return new("DF3dv", s3fseq.Reverse().Select(e => _getStrKeyWithDataPair(e, s3, e)).ToList());
			yield return new("UF3dv", s3fseq.OrderBy(e => r.Next()).Select(e => _getStrKeyWithDataPair(e, s3, e)).ToList());
			yield return new("AVdv", vlseq.Select(e => _getStrKeyWithDataPair(e, e, e)).ToList());
			yield return new("DVdv", vlseq.Reverse().Select(e => _getStrKeyWithDataPair(e, e, e)).ToList());
			yield return new("UVdv", vlseq.OrderBy(e => r.Next()).Select(e => _getStrKeyWithDataPair(e, e, e)).ToList());

			// Mixing external keys with some normal keys
			yield return new("ex+UF3dv", s3fseq.Select(e => _getStrKeyWithDataPair(e, s3, e)).Concat(
					s3fseq.Select(e => _getExternalStrKeyWithDataPair(e, s3, e)))
				.OrderBy(e => r.Next())
			.ToList());


			yield return new("ex+UVdv", vlseq.Select(e => _getStrKeyWithDataPair(e, e, e)).Concat(
					vlseq.Select(e => _getExternalStrKeyWithDataPair(e, e + 1, e)))
				.OrderBy(e => r.Next())
			.ToList());

			// Each type separately
			foreach (var (type, data) in tseqs)
			{
				yield return new(type, [.. data]);
			}

			// All types together
			yield return new("T_ALL", tseqs.SelectMany(e => e.Item2).ToList());
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
