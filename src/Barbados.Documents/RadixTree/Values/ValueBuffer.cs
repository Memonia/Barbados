using System;

namespace Barbados.Documents.RadixTree.Values
{
	internal abstract class ValueBuffer(ValueTypeMarker marker) : IValueBuffer
	{
		public ValueTypeMarker Marker { get; } = marker;

		public abstract int GetLength();
		public abstract object GetValue();

		public abstract void WriteTo(Span<byte> destination);
	}
}
