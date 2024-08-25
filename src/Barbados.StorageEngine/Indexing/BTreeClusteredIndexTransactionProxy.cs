using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed partial class BTreeClusteredIndexTransactionProxy : AbstractBTreeIndexTransactionProxy<ObjectPage>
	{
		private static BTreeIndexKey _toBTreeIndexKey(ReadOnlySpan<byte> objectIdNormalisedBuffer)
		{
			return new(NormalisedValueSpan.FromNormalised(objectIdNormalisedBuffer), false);
		}

		public BTreeClusteredIndexTransactionProxy(TransactionScope transaction, BTreeIndexInfo info)
			: base(transaction, info)
		{

		}

		public bool TryReadHandle(ObjectIdNormalised id, out PageHandle handle)
		{
			var ikey = _toBTreeIndexKey(id);
			if (TryFind(ikey, out var traceback))
			{
				handle = traceback.Current;
				return true;
			}

			handle = default!;
			return false;
		}

		public bool TryReadObjectBuffer(ObjectIdNormalised id, ValueSelector selector, out ObjectBuffer buffer)
		{
			if (!TryReadHandle(id, out var handle))
			{
				buffer = default!;
				return false;
			}

			var page = Transaction.Load<ObjectPage>(handle);
			if (!TryReadObjectBuffer(id, page, selector, out buffer))
			{
				return false;
			}

			return true;
		}
	
		public bool TryReadObjectBuffer(ObjectIdNormalised id, ObjectPage page, ValueSelector selector, out ObjectBuffer buffer)
		{
			if (page.TryReadObject(id, out var bytes))
			{
				buffer = selector.All
					? new ObjectBuffer(bytes.ToArray())
					: ObjectBuffer.Select(bytes, selector);

				return true;
			}

			else
			if (page.TryReadObjectChunk(id, out var chunk, out var totalLength, out var next))
			{
				var read = 0;
				var bufferArr = new byte[totalLength];
				var bufferSpan = bufferArr.AsSpan();

				chunk.CopyTo(bufferSpan[read..]);
				read += chunk.Length;
				while (!next.IsNull && read < totalLength)
				{
					var opage = Transaction.Load<ObjectPageOverflow>(next);
					var r = opage.TryReadObjectChunk(id, out chunk);
					Debug.Assert(r);

					chunk.CopyTo(bufferSpan[read..]);
					read += chunk.Length;
					Debug.Assert(read <= totalLength);
					next = opage.Next;
				}

				buffer = selector.All
					? new ObjectBuffer(bufferArr)
					: ObjectBuffer.Select(bufferArr, selector);

				return true;
			}

			buffer = default!;
			return false;
		}

		public bool TryGetLeftmostLeafHandle(out PageHandle handle)
		{
			var min = new ObjectIdNormalised(ObjectId.MinValue);
			var k = _toBTreeIndexKey(min);
			if (TryFind(k, out var traceback))
			{
				handle = traceback.Current;
				return true;
			}

			handle = default!;
			return false;
		}

		public bool TryGetRightmostLeafHandle(out PageHandle handle)
		{
			var max = new ObjectIdNormalised(ObjectId.MaxValue);
			var k = _toBTreeIndexKey(max);
			if (TryFind(k, out var traceback))
			{
				handle = traceback.Current;
				return true;
			}

			handle = default!;
			return false;
		}
	}
}
