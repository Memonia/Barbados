using Barbados.CommonUtils.BitManipulation;
using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents.RadixTree.Metadata
{
	internal readonly struct ValueDescriptor
	{
		public const int BinaryLength = sizeof(uint);

		// Relative to the start of the value table
		private const uint _relativeOffsetMask = 0b0000_0000_1111_1111_1111_1111_1111_1111;
		private const uint _typeMarkerMask     = 0b1111_1111_0000_0000_0000_0000_0000_0000;

		private const int _relativeOffsetShift = 0;
		private const int _typeMarkerShift     = 24;

		public readonly uint Bits => _bits;

		private readonly uint _bits;

		public ValueDescriptor(uint bits)
		{
			_bits = bits;
		}

		public ValueDescriptor(int offset, ValueTypeMarker marker)
		{
			_bits = 0;
			Marker = marker;
			RelativeOffset = offset;
		}

		public ValueTypeMarker Marker
		{
			readonly get => (ValueTypeMarker)_bits.GetBits(_typeMarkerMask, _typeMarkerShift);
			init => _bits.SetBits((byte)value, _typeMarkerMask, _typeMarkerShift);
		}

		public int RelativeOffset
		{
			readonly get => (int)_bits.GetBits(_relativeOffsetMask, _relativeOffsetShift);
			init => _bits.SetBits((uint)value, _relativeOffsetMask, _relativeOffsetShift);
		}
	}
}
