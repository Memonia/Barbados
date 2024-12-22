using System;

namespace Barbados.Documents.Serialisation.Values.ValueBufferSpanComparers
{
	internal sealed class ArrayStringBufferSpanComparer : IValueBufferSpanComparer
	{
		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			var xcount = ValueBufferRawHelpers.GetArrayBufferCount(x);
			var ycount = ValueBufferRawHelpers.GetArrayBufferCount(y);
			for (int i = 0; i < xcount && i < ycount; ++i)
			{
				var xstr = ValueBufferRawHelpers.GetStringBytesFromArray(x, i);
				var ystr = ValueBufferRawHelpers.GetStringBytesFromArray(y, i);
				var c = xstr.SequenceCompareTo(ystr);
				if (c != 0)
				{
					return c;
				}
			}

			return xcount - ycount;
		}
	}
}
