using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Barbados.StorageEngine.BTree.Pages;
using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine.BTree
{
	internal partial class BTreeContext
	{
		/* TODO: chunk operations could be improved. For now, each operation involving overflowed data
		 * must walk the whole tree for each chunk. Chunk keys for the same object only differ by an 
		 * index and so they are stored sequentially. Knowing that we could do range operations on
		 * chunks to improve performance
		 */

		private bool _tryReadChunkedData(BTreeLookupKeySpan lookupKey, long sequenceNumber, ChunkType type, out byte[] data)
		{
			Span<byte> cks = stackalloc byte[ChunkKey.GetLength(lookupKey)];
			var chunkKey = new ChunkKey(cks, lookupKey, sequenceNumber, type);

			var chunkIndex = 0;
			chunkKey.SetIndex(chunkIndex);

			var cklk = _toLookupKey(new InternalLookupKeySpan(chunkKey.AsNormalised(), isChunkKey: true), out _);
			if (!_tryFind(cklk, out var traceback))
			{
				data = default!;
				return false;
			}

			var page = Transaction.Load<BTreeLeafPage>(traceback.Current);
			var chunks = new List<byte[]>();
			var wholeDataLength = 0;
			while (true)
			{
				if (!page.TryReadData(cklk, out var chunk))
				{
					if (page.Next.IsNull)
					{
						break;
					}

					page = Transaction.Load<BTreeLeafPage>(page.Next);
					if (!page.TryReadData(cklk, out _))
					{
						break;
					}
				}

				else
				{
					chunks.Add(chunk.ToArray());
					wholeDataLength += chunk.Length;
					chunkIndex += 1;
					chunkKey.SetIndex(chunkIndex);
				}
			}

			// TODO: reduce memory allocations. Get 'wholeDataLength' in O(1),
			// allocate the resulting array once and write bytes as the chunks are being read
			data = new byte[wholeDataLength];
			var written = 0;
			var dspan = data.AsSpan();
			foreach (var chunk in chunks)
			{
				chunk.CopyTo(dspan[written..]);
				written += chunk.Length;
			}

			return true;
		}

		private void _writeChunkedData(BTreeLookupKeySpan lookupKey, long sequenceNumber, ReadOnlySpan<byte> data, ChunkType type)
		{
			Span<byte> cks = stackalloc byte[ChunkKey.GetLength(lookupKey)];
			var chunkKey = new ChunkKey(cks, lookupKey, sequenceNumber, type);

			var remaining = data.Length;
			var count = data.Length / Info.MaxDataLength;
			if (data.Length % Info.MaxDataLength > 0)
			{
				count += 1;
			}

			// Insert chunks in descending order to minimise the number of parent updates
			while (count > 0)
			{
				var i = count - 1;
				var chunk = remaining - Info.MaxDataLength < 0
					? data[..remaining]
					: data.Slice(remaining - Info.MaxDataLength, Info.MaxDataLength);

				chunkKey.SetIndex(i);
				if (!_tryInsert(new(chunkKey.AsNormalised(), isChunkKey: true), chunk))
				{
					// For each unique key the chunk key should also be unique
					throw new BarbadosInternalErrorException();
				}

				count -= 1;
				remaining -= chunk.Length;
			}

			Debug.Assert(remaining == 0);
		}

		private void _removeChunkedData(BTreeLookupKeySpan lookupKey, long sequenceNumber, ChunkType type)
		{
			Span<byte> cks = stackalloc byte[ChunkKey.GetLength(lookupKey)];
			var chunkKey = new ChunkKey(cks, lookupKey, sequenceNumber, type);

			// Remmove chunks in ascending order to minimise the number of parent updates
			var index = 0;
			scoped InternalLookupKeySpan rmkey;
			do
			{
				chunkKey.SetIndex(index);
				index += 1;
				rmkey = new InternalLookupKeySpan(BTreeNormalisedValueSpan.FromNormalised(cks), isChunkKey: true);
			}
			while (_tryRemove(rmkey));
		}

		private bool _tryGetKeySequenceNumber(BTreeNormalisedValueSpan key, out long sequenceNumber, out long firstGap)
		{
			if (!_tryGetOverflowInfo(key, out var info))
			{
				sequenceNumber = -1;
				firstGap = -1;
				return false;
			}

			var lookupKey = _toLookupKey(key, out var remainder);

			Span<byte> cks = stackalloc byte[ChunkKey.GetLength(lookupKey)];
			var chunkKey = new ChunkKey(cks, lookupKey, sequenceNumber: 0, ChunkType.KeyChunk);

			// We probe for existence of the sequence with a specific number by searching for its first chunk
			chunkKey.SetIndex(0);

			firstGap = -1;
			sequenceNumber = 0;
			while (sequenceNumber < info.NextSequenceNumber)
			{
				chunkKey.SetSequenceNumber(sequenceNumber);
				if (_tryReadChunkedData(lookupKey, sequenceNumber, ChunkType.KeyChunk, out var currentRemainder))
				{
					if (currentRemainder.AsSpan().SequenceEqual(remainder))
					{
						return true;
					}
				}

				else
				{
					if (firstGap < 0)
					{
						firstGap = sequenceNumber;
					}
				}

				sequenceNumber += 1;
			}

			sequenceNumber = -1;
			return false;
		}

		private bool _tryGetOverflowInfo(BTreeNormalisedValueSpan key, out BTreeLeafPage.OverflowInfo info)
		{
			var lookupKey = _toLookupKey(key, out _);
			if (!_tryFind(lookupKey, out var traceback))
			{
				info = default!;
				return false;
			}

			var page = Transaction.Load<BTreeLeafPage>(traceback.Current);
			return page.TryReadOverflowInfo(lookupKey, out info);
		}
	}
}
