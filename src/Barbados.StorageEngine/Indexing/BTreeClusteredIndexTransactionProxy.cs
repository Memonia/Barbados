using System;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Serialisation;
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

		public bool TryReadObjectBuffer(ObjectIdNormalised id, ValueSelector selector, out RadixTreeBuffer buffer)
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

		public bool TryReadObjectBuffer(ObjectIdNormalised id, ObjectPage page, ValueSelector selector, out RadixTreeBuffer buffer)
		{
			static RadixTreeBuffer _selectFromBuffer(ReadOnlySpan<byte> buffer, ValueSelector selector)
			{
				Debug.Assert(!selector.All);
				var builder = new RadixTreeBuffer.Builder();
				foreach (var identifier in selector)
				{
					if (identifier.IsDocument)
					{
						var e = new RadixTreeBuffer.KeyValueEnumerator(buffer);
						while (e.TryGetNext(out var key, out var value))
						{
							builder.AddBuffer(key.ToString(), value);
						}
					}

					else
					{
						if (RadixTreeBuffer.TryGetBuffer(buffer, identifier.BinaryName.AsBytes(), out var valueBuffer))
						{
							builder.AddBuffer(identifier, valueBuffer);
						}
					}
				}

				return builder.Build();
			}

			if (page.TryReadObject(id, out var bytes))
			{
				buffer = selector.All
					? new RadixTreeBuffer(bytes.ToArray())
					: _selectFromBuffer(bytes, selector);

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
					? new RadixTreeBuffer(bufferArr)
					: _selectFromBuffer(bufferArr, selector);

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
