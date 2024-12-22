using System;
using System.Diagnostics;

using Barbados.Documents;
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

		public bool TryReadDocument(ObjectIdNormalised id, BarbadosKeySelector selector, out BarbadosDocument document)
		{
			if (!TryReadHandle(id, out var handle))
			{
				document = default!;
				return false;
			}

			var page = Transaction.Load<ObjectPage>(handle);
			if (!TryReadDocument(id, page, selector, out document))
			{
				return false;
			}

			return true;
		}

		public bool TryReadDocument(ObjectIdNormalised id, ObjectPage page, BarbadosKeySelector selector, out BarbadosDocument document)
		{
			if (page.TryReadObject(id, out var bytes))
			{
				document = selector.All
					? BarbadosDocument.Builder.FromBytes(bytes)
					: BarbadosDocument.Builder.FromBytes(bytes, selector);

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

				document = selector.All
					? BarbadosDocument.Builder.FromBytes(bufferArr)
					: BarbadosDocument.Builder.FromBytes(bufferArr, selector);

				return true;
			}

			document = default!;
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
