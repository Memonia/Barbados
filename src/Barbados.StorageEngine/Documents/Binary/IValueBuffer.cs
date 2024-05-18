using System;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal interface IValueBuffer
	{
		bool IsArray { get; }
		ValueTypeMarker Marker { get; }

		int GetLength();
		void WriteTo(Span<byte> destination);
	}
}
