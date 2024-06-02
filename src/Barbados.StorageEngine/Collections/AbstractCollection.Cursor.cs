using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Collections
{
	internal partial class AbstractCollection : IBarbadosCollection
	{
		public ICursor<BarbadosDocument> GetCursor()
		{
			return GetCursor(ValueSelector.SelectAll);
		}

		public ICursor<BarbadosDocument> GetCursor(ValueSelector selector)
		{
			IEnumerable<BarbadosDocument> _enum()
			{
				IEnumerable<BarbadosDocument> _retrieve(List<(ObjectId, ObjectBuffer)> buffer, ObjectPage page)
				{
					var e = page.GetEnumerator();
					while (e.TryGetNext(out var id))
					{
						var or = ObjectReader.TryRead(Pool, page, id, selector, out var obj);
						Debug.Assert(or);

						buffer.Add((id, obj));
					}

					foreach (var (id, obj) in buffer)
					{
						yield return new BarbadosDocument(id, obj);
					}

					buffer.Clear();
				}

				var buffer = new List<(ObjectId, ObjectBuffer)>();
				var page = Pool.LoadPin<ObjectPage>(CollectionPageHandle);
				var next = page.Next;
				var previous = page.Previous;
				foreach (var doc in _retrieve(buffer, page))
				{
					yield return doc;
				}

				Pool.Release(page);
				while (!next.IsNull)
				{
					page = Pool.LoadPin<ObjectPage>(next);
					foreach (var doc in _retrieve(buffer,page))
					{
						yield return doc;
					}

					next = page.Next;
					Pool.Release(page);
				}

				while (!previous.IsNull)
				{
					page = Pool.LoadPin<ObjectPage>(previous);
					foreach (var doc in _retrieve(buffer, page))
					{
						yield return doc;
					}

					previous = page.Previous;
					Pool.Release(page);
				}
			}

			return new Cursor<BarbadosDocument>(_enum(), Lock);
		}
	}
}
