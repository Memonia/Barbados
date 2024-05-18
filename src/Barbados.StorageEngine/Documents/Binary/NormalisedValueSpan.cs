using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal readonly ref struct NormalisedValueSpan
	{
		public static NormalisedValueSpan FromNormalised(ReadOnlySpan<byte> span) => new(span);

		public ReadOnlySpan<byte> Bytes { get; }

		public NormalisedValueSpan(NormalisedValue value) : this(value.AsSpan().Bytes)
		{

		}

		private NormalisedValueSpan(ReadOnlySpan<byte> value)
		{
			Bytes = value;
		}
	}
}
