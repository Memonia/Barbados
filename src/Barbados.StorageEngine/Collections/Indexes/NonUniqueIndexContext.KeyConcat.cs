using System;

using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Collections.Indexes
{
	internal partial class NonUniqueIndexContext
	{
		private readonly ref struct KeyConcat
		{
			// The format of the key is [indexKey][primaryKey]. When we execute range query, we first collect all
			// entries with [indexKey] portion as a prefix, retrieve the length of the index key portion from the
			// entry and then use it to filter out the entries whose match was caused by a portion of a primary key
			// and not the index key.
			//
			// Both primary and index keys are normalised BTree values
			public ReadOnlySpan<byte> Bytes { get; }

			public KeyConcat(ReadOnlySpan<byte> bytes)
			{
				Bytes = bytes;
			}

			public KeyConcat(BTreeNormalisedValueSpan primaryKey, BTreeNormalisedValueSpan key)
			{
				// TODO: reduce memory allocations. For small key lengths we could stackalloc
				Span<byte> bytes = new byte[primaryKey.Bytes.Length + key.Bytes.Length];
				var i = 0;
				key.Bytes.CopyTo(bytes[i..]);
				i += key.Bytes.Length;
				primaryKey.Bytes.CopyTo(bytes[i..]);

				Bytes = bytes;
			}

			public ReadOnlySpan<byte> GetIndexKeyPortion(int indexPortionLength)
			{
				return Bytes[..indexPortionLength];
			}

			public ReadOnlySpan<byte> GetPrimaryKeyPortion(int indexPortionLength)
			{
				return Bytes[indexPortionLength..];
			}
		}
	}
}
