using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Collections
{
	internal static class ObjectReader
	{
		public static bool TryRead(PagePool pool, PageHandle handle, ObjectId id, ValueSelector selector, out ObjectBuffer obj)
		{
			var page = pool.LoadPin<ObjectPage>(handle);
			var r = TryRead(pool, page, id, selector, out obj);
			pool.Release(page);
			return r;
		}

		public static bool TryRead(PagePool pool, ObjectPage page, ObjectId id, ValueSelector selector, out ObjectBuffer obj)
		{
			var idn = new ObjectIdNormalised(id);
			if (page.TryReadObject(idn, out var bytes))
				{
				obj = selector.All
					? new ObjectBuffer(bytes.ToArray())
					: ObjectBuffer.Select(bytes, selector);

				return true;
			}

			else
			if (page.TryReadObjectChunk(idn, out var chunk, out var totalLength, out var next))
			{
				var read = 0;
				var buffer = new byte[totalLength];
				var bufferSpan = buffer.AsSpan();

				chunk.CopyTo(bufferSpan[read..]);
				read += chunk.Length;
				while (!next.IsNull && read < totalLength)
				{
					var opage = pool.LoadPin<ObjectPageOverflow>(next);
					var r = opage.TryReadObjectChunk(idn, out chunk);
					Debug.Assert(r);

					chunk.CopyTo(bufferSpan[read..]);
					read += chunk.Length;
					Debug.Assert(read <= totalLength);

					next = opage.Next;
					pool.Release(opage);
				}

				obj = selector.All
					? new ObjectBuffer(buffer)
					: ObjectBuffer.Select(buffer, selector);

				return true;
			}

			else
			{
				obj = default!;
				return false;
			}
		}
	}
}
