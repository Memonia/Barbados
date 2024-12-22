using System;

namespace Barbados.Documents.Serialisation.Values
{
	internal interface IValueBuffer
	{
		ValueTypeMarker Marker { get; }

		int GetLength();
		object GetValue();

		void WriteTo(Span<byte> destination);
	}
}
