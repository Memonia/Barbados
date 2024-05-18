using System;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueStringBuffer : ValueVariableLengthBuffer<string>
	{
		public ValueStringBuffer(string value) : this(value, ValueBufferRawHelpers.GetLength(value))
		{

		}

		private ValueStringBuffer(string value, int length) : base(value, length, ValueTypeMarker.String)
		{

		}

		public override void WriteValueTo(Span<byte> destination)
		{
			ValueBufferRawHelpers.WriteStringValue(destination, Value);
		}
	}
}
