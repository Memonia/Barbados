using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal abstract class ValueBuffer(ValueTypeMarker marker) : IValueBuffer
	{
		public ValueTypeMarker Marker { get; } = marker;

		public abstract int GetLength();

		public abstract void WriteTo(Span<byte> destination);
	}
}
