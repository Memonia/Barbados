using System;

namespace Barbados.StorageEngine.Documents.Serialisation.Values
{
	internal interface IValueBuffer
	{
		ValueTypeMarker Marker { get; }

		int GetLength();
		void WriteTo(Span<byte> destination);
	}
}
