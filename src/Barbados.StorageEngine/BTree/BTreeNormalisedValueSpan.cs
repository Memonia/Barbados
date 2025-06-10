using System;

namespace Barbados.StorageEngine.BTree
{
	internal readonly ref struct BTreeNormalisedValueSpan
	{
		public static BTreeNormalisedValueSpan FromNormalised(ReadOnlySpan<byte> span) => new(span);

		public BTreeLookupKeyTypeMarker Marker => (BTreeLookupKeyTypeMarker)Bytes[0];
		public ReadOnlySpan<byte> Bytes { get; }

		private BTreeNormalisedValueSpan(ReadOnlySpan<byte> value)
		{
			Bytes = value;
		}
	}
}
