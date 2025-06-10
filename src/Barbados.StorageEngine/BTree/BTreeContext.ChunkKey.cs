using System;
using System.Diagnostics;

namespace Barbados.StorageEngine.BTree
{
	internal partial class BTreeContext
	{
		private readonly ref struct ChunkKey
		{
			public const int OverheadPerLookupKeyLength = sizeof(byte) + sizeof(long) + sizeof(int);

			public static int GetLength(BTreeLookupKeySpan lookupKey)
			{
				return sizeof(byte) + lookupKey.Separator.Bytes.Length + sizeof(long) + sizeof(int);
			}

			// The format of the chunk key is [chunkMarker][lookupKey][sequenceNumber][chunkIndex]
			private readonly Span<byte> _key;
			private readonly Span<byte> _indexPortion;
			private readonly Span<byte> _sequenceNumberPortion;

			public ChunkKey(Span<byte> span, BTreeLookupKeySpan lookupKey, long sequenceNumber, ChunkType type)
			{
				Debug.Assert(span.Length <= BTreeInfo.LimitMaxLookupKeyLength);

				_key = span;
				_indexPortion = _key[(sizeof(byte) + lookupKey.Separator.Bytes.Length + sizeof(long))..];
				_sequenceNumberPortion = _key.Slice(sizeof(byte) + lookupKey.Separator.Bytes.Length, sizeof(long));

				SetType(type);
				SetSequenceNumber(sequenceNumber);
				SetIndex(0);

				lookupKey.Separator.Bytes.CopyTo(_key.Slice(sizeof(byte), lookupKey.Separator.Bytes.Length));
			}

			public void SetType(ChunkType type)
			{
				var _ = type switch
				{
					ChunkType.KeyChunk => _key[0] = (byte)BTreeLookupKeyTypeMarker.KeyChunk,
					ChunkType.DataChunk => _key[0] = (byte)BTreeLookupKeyTypeMarker.DataChunk,
					_ => throw new NotImplementedException(),
				};
			}

			public void SetIndex(int index)
			{
				BTreeNormalisedValue.WriteWithoutMarker(index, _indexPortion);
			}

			public void SetSequenceNumber(long sequenceNumber)
			{
				BTreeNormalisedValue.WriteWithoutMarker(sequenceNumber, _sequenceNumberPortion);
			}

			public BTreeNormalisedValueSpan AsNormalised()
			{
				return BTreeNormalisedValueSpan.FromNormalised(_key);
			}
		}
	}
}
