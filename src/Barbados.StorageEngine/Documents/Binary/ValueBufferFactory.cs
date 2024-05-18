using System;

using Barbados.StorageEngine.Documents.Binary.ValueBuffers;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal static class ValueBufferFactory
	{
		public static IValueBuffer CreateFromRawBuffer(ReadOnlySpan<byte> buffer, ValueTypeMarker marker)
		{
			return marker switch
			{
				ValueTypeMarker.Int8 => new ValueInt8Buffer(ValueBufferRawHelpers.ReadInt8(buffer)),
				ValueTypeMarker.Int16 => new ValueInt16Buffer(ValueBufferRawHelpers.ReadInt16(buffer)),
				ValueTypeMarker.Int32 => new ValueInt32Buffer(ValueBufferRawHelpers.ReadInt32(buffer)),
				ValueTypeMarker.Int64 => new ValueInt64Buffer(ValueBufferRawHelpers.ReadInt64(buffer)),
				ValueTypeMarker.UInt8 => new ValueUInt8Buffer(ValueBufferRawHelpers.ReadUInt8(buffer)),
				ValueTypeMarker.UInt16 => new ValueUInt16Buffer(ValueBufferRawHelpers.ReadUInt16(buffer)),
				ValueTypeMarker.UInt32 => new ValueUInt32Buffer(ValueBufferRawHelpers.ReadUInt32(buffer)),
				ValueTypeMarker.UInt64 => new ValueUInt64Buffer(ValueBufferRawHelpers.ReadUInt64(buffer)),
				ValueTypeMarker.Float32 => new ValueFloat32Buffer(ValueBufferRawHelpers.ReadFloat32(buffer)),
				ValueTypeMarker.Float64 => new ValueFloat64Buffer(ValueBufferRawHelpers.ReadFloat64(buffer)),
				ValueTypeMarker.Boolean => new ValueBooleanBuffer(ValueBufferRawHelpers.ReadBoolean(buffer)),
				ValueTypeMarker.DateTime => new ValueDateTimeBuffer(ValueBufferRawHelpers.ReadDateTime(buffer)),
				ValueTypeMarker.String => new ValueStringBuffer(ValueBufferRawHelpers.ReadStringFromBuffer(buffer)),
				_ => throw new NotImplementedException()
			};
		}

		public static IValueBuffer CreateFromRawBufferArray(ReadOnlySpan<byte> buffer, ValueTypeMarker marker)
		{
			return marker switch
			{
				ValueTypeMarker.Int8 => new ValueInt8BufferArray(ValueBufferRawHelpers.ReadInt8Array(buffer)),
				ValueTypeMarker.Int16 => new ValueInt16BufferArray(ValueBufferRawHelpers.ReadInt16Array(buffer)),
				ValueTypeMarker.Int32 => new ValueInt32BufferArray(ValueBufferRawHelpers.ReadInt32Array(buffer)),
				ValueTypeMarker.Int64 => new ValueInt64BufferArray(ValueBufferRawHelpers.ReadInt64Array(buffer)),
				ValueTypeMarker.UInt8 => new ValueUInt8BufferArray(ValueBufferRawHelpers.ReadUInt8Array(buffer)),
				ValueTypeMarker.UInt16 => new ValueUInt16BufferArray(ValueBufferRawHelpers.ReadUInt16Array(buffer)),
				ValueTypeMarker.UInt32 => new ValueUInt32BufferArray(ValueBufferRawHelpers.ReadUInt32Array(buffer)),
				ValueTypeMarker.UInt64 => new ValueUInt64BufferArray(ValueBufferRawHelpers.ReadUInt64Array(buffer)),
				ValueTypeMarker.Float32 => new ValueFloat32BufferArray(ValueBufferRawHelpers.ReadFloat32Array(buffer)),
				ValueTypeMarker.Float64 => new ValueFloat64BufferArray(ValueBufferRawHelpers.ReadFloat64Array(buffer)),
				ValueTypeMarker.Boolean => new ValueBooleanBufferArray(ValueBufferRawHelpers.ReadBooleanArray(buffer)),
				ValueTypeMarker.DateTime => new ValueDateTimeBufferArray(ValueBufferRawHelpers.ReadDateTimeArray(buffer)),
				ValueTypeMarker.String => new ValueStringBufferArray(ValueBufferRawHelpers.ReadStringArray(buffer)),
				_ => throw new NotImplementedException()
			};
		}
	}
}
