using System;
using System.Linq;

namespace Barbados.StorageEngine.Documents.Binary.ValueBuffers
{
	internal sealed class ValueStringBufferArray : ValueVariableLengthBufferArray<string>
	{
		public ValueStringBufferArray(string[] values) :
			base(values, values.Select(e => e.Length).ToArray(), ValueTypeMarker.String)
		{

		}

		public override void WriteValueTo(Span<byte> destination, int index)
		{
			ValueBufferRawHelpers.WriteStringValue(destination, Values[index]);
		}
	}
}
