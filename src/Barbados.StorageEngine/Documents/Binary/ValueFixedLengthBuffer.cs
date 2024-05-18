using System.Diagnostics;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal abstract class ValueFixedLengthBuffer<T> : ValueBuffer
	{
		public T Value { get; }
		public int ValueLength { get; }

		public ValueFixedLengthBuffer(T value, ValueTypeMarker marker) : base(marker, false)
		{
			Debug.Assert(!marker.IsVariableLength());

			Value = value;
			ValueLength = marker.GetFixedLength();
		}

		public override int GetLength()
		{
			return ValueLength;
		}
	}
}
