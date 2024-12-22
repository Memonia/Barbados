using System;
using System.Diagnostics;

namespace Barbados.Documents.Serialisation.Values
{
	internal sealed class FixedLengthTypeArrayBufferSpanComparer<T> : IValueBufferSpanComparer
		where T : IComparable<T>
	{
		private readonly int _valueLength;
		private readonly FixedLengthTypeValueBufferSpanComparer<T> _comparer;

		public FixedLengthTypeArrayBufferSpanComparer(ValueTypeMarker marker, FixedLengthTypeValueBufferSpanComparer<T> comparer)
		{
			Debug.Assert(marker.IsArray());
			Debug.Assert(!marker.IsVariableLengthTypeArray());

			_valueLength = marker.GetFixedLengthTypeLength();
			_comparer = comparer;
		}

		public int Compare(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
		{
			var xcount = ValueBufferRawHelpers.GetArrayBufferCount(x);
			var ycount = ValueBufferRawHelpers.GetArrayBufferCount(y);
			var count = 0;
			for (int i = 0; i < xcount && i < ycount; ++i)
			{
				var offset = count * _valueLength;
				var c = _comparer.Compare(x[offset..], y[offset..]);
				if (c != 0)
				{
					return c;
				}
			}

			return xcount - ycount;
		}
	}
}
