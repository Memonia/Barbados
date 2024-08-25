using System;
using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	internal sealed class BTreeIndexFacadeTestSequenceProvider : TheoryData<BTreeIndexFacadeTestSequence>
	{
		public BTreeIndexFacadeTestSequenceProvider()
		{
			static BarbadosDocument _padded(BarbadosDocument.Builder builder, string field, int value, int width)
			{
				var str = Convert.ToString(value).PadLeft(width, '0');
				return builder.Add(field, str).Build();
			}
 
			var maxLen = Constants.IndexKeyMaxLength;
			var minLen = Constants.MinIndexKeyMaxLength;
			var s1 = maxLen / 100;
			var s2 = maxLen / 10;
			var s3 = maxLen / 1;
			
			Debug.Assert(s1 > 0);
			Debug.Assert(s2 > 0);
			Debug.Assert(s3 > 0);

			var r = new XorShiftStar32(271828182);
			var r1 = Enumerable.Range(1, s1).ToArray();
			var r2 = Enumerable.Range(1, s2).ToArray();
			var r3 = Enumerable.Range(1, s3).ToArray();
			var c1 = Enumerable.Repeat(r1, 100).SelectMany(e => e).ToArray();
			var c2 = Enumerable.Repeat(r2, 10).SelectMany(e => e).ToArray();
			var c3 = Enumerable.Repeat(r3, 2).SelectMany(e => e).ToArray();

			var builder = new BarbadosDocument.Builder();
			var field = "index-test";

			/* Test whether an index handles splits/merges correctly. 
			 * Generated keys are long strings, which helps creating more splits.
			 * 
			 * UAF3 and DAF3 are left out, because they run very slowly due to too many splits
			 */

			// Unique, ascending, fixed length
			Add(new(field, maxLen, new("UAF1", r1.Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("UAF2", r2.Select(e => _padded(builder, field, e, maxLen)).ToArray())));

			// Unique, descending, fixed length
			Add(new(field, maxLen, new("UDF1", r1.Reverse().Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("UDF2", r2.Reverse().Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("UDF3", r3.Reverse().Select(e => _padded(builder, field, e, maxLen)).ToArray())));

			// Unique, unordered, fixed length
			Add(new(field, maxLen, new("UUF1", r1.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("UUF2", r2.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("UUF3", r3.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, maxLen)).ToArray())));

			// Unique, ascending, variable length
			Add(new(field, maxLen, new("UAV1", r1.Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("UAV2", r2.Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("UAV3", r3.Select(e => _padded(builder, field, e, e)).ToArray())));

			// Unique, descending, variable length
			Add(new(field, maxLen, new("UDV1", r1.Reverse().Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("UDV2", r2.Reverse().Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("UDV3", r3.Reverse().Select(e => _padded(builder, field, e, e)).ToArray())));

			// Unique, unordered, variable length
			Add(new(field, maxLen, new("UUV1", r1.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("UUV2", r2.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("UUV3", r3.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, e)).ToArray())));

			// Duplicates, ascending, fixed length
			Add(new(field, maxLen, new("DAF1", c1.Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("DAF2", c2.Select(e => _padded(builder, field, e, maxLen)).ToArray())));

			// Duplicates, descending, fixed length
			Add(new(field, maxLen, new("DDF1", c1.Reverse().Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("DDF2", c2.Reverse().Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("DDF3", c3.Reverse().Select(e => _padded(builder, field, e, maxLen)).ToArray())));

			// Duplicates, unordered, fixed length
			Add(new(field, maxLen, new("DUF1", c1.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("DUF2", c2.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, maxLen)).ToArray())));
			Add(new(field, maxLen, new("DUF3", c3.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, maxLen)).ToArray())));

			// Duplicates, ascending, variable length
			Add(new(field, maxLen, new("DAV1", c1.Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("DAV2", c2.Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("DAV3", c3.Select(e => _padded(builder, field, e, e)).ToArray())));

			// Duplicates, descending, variable length
			Add(new(field, maxLen, new("DDV1", c1.Reverse().Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("DDV2", c2.Reverse().Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("DDV3", c3.Reverse().Select(e => _padded(builder, field, e, e)).ToArray())));

			// Duplicates, unordered, variable length
			Add(new(field, maxLen, new("DUV1", c1.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("DUV2", c2.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, e)).ToArray())));
			Add(new(field, maxLen, new("DUV3", c3.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, e)).ToArray())));

			/* Test whether an index handles other types correctly
			 */

			var docs = new BarbadosDocument[][]
			{
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (sbyte)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (short)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (int)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (long)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (byte)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (ushort)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (uint)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (ulong)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (float)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (double)e).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, new DateTime(e)).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => builder.Add(field, (e & 1) == 0).Build()).ToArray(),
				r1.OrderBy(e => r.Next()).Select(e => _padded(builder, field, e, e)).ToArray()
			};

			Add(new(field, new("T01", docs[0])));
			Add(new(field, new("T02", docs[1])));
			Add(new(field, new("T03", docs[2])));
			Add(new(field, new("T04", docs[3])));
			Add(new(field, new("T05", docs[4])));
			Add(new(field, new("T06", docs[5])));
			Add(new(field, new("T07", docs[6])));
			Add(new(field, new("T08", docs[7])));
			Add(new(field, new("T09", docs[8])));
			Add(new(field, new("T10", docs[9])));
			Add(new(field, new("T11", docs[10])));
			Add(new(field, new("T12", docs[11])));

			// Multiple different types in the same index
			Add(new(field, maxLen, new("TM1", docs.SelectMany(e => e))));

			// Same as above, but cut key length to the minimum
			Add(new(field, minLen, new("TM2", docs.SelectMany(e => e))));
		}
	}
}
