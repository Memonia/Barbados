using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal abstract class ValueBuffer(ValueTypeMarker marker, bool isArray) : IValueBuffer
	{
		public bool IsArray { get; } = isArray;
		public ValueTypeMarker Marker { get; } = marker;

		public abstract int GetLength();

		public abstract void WriteTo(Span<byte> destination);
	}
}
