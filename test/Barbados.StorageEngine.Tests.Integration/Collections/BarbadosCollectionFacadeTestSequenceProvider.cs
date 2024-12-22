using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	internal sealed class BarbadosCollectionFacadeTestSequenceProvider : IEnumerable<BarbadosCollectionFacadeTestSequence>
	{
		private static BarbadosDocument _get(BarbadosDocument.Builder builder, int count)
		{
			for (int i = 0; i < count; ++i)
			{
				builder.Add(i.ToString(), i);
			}

			return builder.Build();
		}

		public IEnumerator<BarbadosCollectionFacadeTestSequence> GetEnumerator()
		{
			var db = new BarbadosDocument.Builder();
			var r = new XorShiftStar32(314159265);
			var r1 = Enumerable.Range(1, 10).ToArray();
			var r2 = Enumerable.Range(1, 100).ToArray();
			var r3 = Enumerable.Range(1, 1000).ToArray();

			// Fixed length
			yield return new("F1", r2.OrderBy(e => r.Next()).Select(e => _get(db, 10)).ToArray());
			yield return new("F2", r2.OrderBy(e => r.Next()).Select(e => _get(db, 100)).ToArray());
			yield return new("F3", r2.OrderBy(e => r.Next()).Select(e => _get(db, 1000)).ToArray());

			// Variable length
			yield return new("V1", r1.OrderBy(e => r.Next()).Select(e => _get(db, e)).ToArray());
			yield return new("V2", r1.OrderBy(e => r.Next()).Select(e => _get(db, e + 16)).ToArray());
			yield return new("V3", r1.OrderBy(e => r.Next()).Select(e => _get(db, e + 32)).ToArray());
			yield return new("V4", r2.OrderBy(e => r.Next()).Select(e => _get(db, e)).ToArray());
			yield return new("V5", r2.OrderBy(e => r.Next()).Select(e => _get(db, e + 128)).ToArray());
			yield return new("V6", r2.OrderBy(e => r.Next()).Select(e => _get(db, e + 256)).ToArray());
			yield return new("V7", r3.OrderBy(e => r.Next()).Select(e => _get(db, e)).ToArray());
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
