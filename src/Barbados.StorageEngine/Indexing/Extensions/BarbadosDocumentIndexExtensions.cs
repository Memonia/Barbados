using System;

using Barbados.StorageEngine.Documents.Serialisation;
using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Indexing.Extensions
{
	internal static class BarbadosDocumentIndexExtensions
	{
		public static bool TryGetNormalisedValue(this RadixTreeBuffer buffer, RadixTreePrefix prefix, out NormalisedValue value)
		{
			if (buffer.TryGetBufferRaw(prefix.AsBytes(), out var marker, out var valueBuffer))
			{
				value = marker switch
				{
					ValueTypeMarker.Int8 => NormalisedValue.Create(ValueBufferRawHelpers.ReadInt8(valueBuffer)),
					ValueTypeMarker.Int16 => NormalisedValue.Create(ValueBufferRawHelpers.ReadInt16(valueBuffer)),
					ValueTypeMarker.Int32 => NormalisedValue.Create(ValueBufferRawHelpers.ReadInt32(valueBuffer)),
					ValueTypeMarker.Int64 => NormalisedValue.Create(ValueBufferRawHelpers.ReadInt64(valueBuffer)),
					ValueTypeMarker.UInt8 => NormalisedValue.Create(ValueBufferRawHelpers.ReadUInt8(valueBuffer)),
					ValueTypeMarker.UInt16 => NormalisedValue.Create(ValueBufferRawHelpers.ReadUInt16(valueBuffer)),
					ValueTypeMarker.UInt32 => NormalisedValue.Create(ValueBufferRawHelpers.ReadUInt32(valueBuffer)),
					ValueTypeMarker.UInt64 => NormalisedValue.Create(ValueBufferRawHelpers.ReadUInt64(valueBuffer)),
					ValueTypeMarker.Float32 => NormalisedValue.Create(ValueBufferRawHelpers.ReadFloat32(valueBuffer)),
					ValueTypeMarker.Float64 => NormalisedValue.Create(ValueBufferRawHelpers.ReadFloat64(valueBuffer)),
					ValueTypeMarker.DateTime => NormalisedValue.Create(ValueBufferRawHelpers.ReadDateTime(valueBuffer)),
					ValueTypeMarker.Boolean => NormalisedValue.Create(ValueBufferRawHelpers.ReadBoolean(valueBuffer)),
					ValueTypeMarker.String => NormalisedValue.Create(ValueBufferRawHelpers.ReadStringFromBuffer(valueBuffer)),
					_ => throw new NotImplementedException()
				};

				return true;
			}

			value = default!;
			return false;
		}
	}
}
