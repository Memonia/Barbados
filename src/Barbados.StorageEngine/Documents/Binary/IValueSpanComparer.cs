using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal interface IValueSpanComparer
	{
		int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y);
	}
}
