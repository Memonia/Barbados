using System;

namespace Barbados.Documents.RadixTree.Values
{
	internal interface IValueBuffer
	{
		ValueTypeMarker Marker { get; }

		int GetLength();
		object GetValue();

		void WriteTo(Span<byte> destination);
	}
}
