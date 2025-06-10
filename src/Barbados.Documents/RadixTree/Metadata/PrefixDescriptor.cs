using Barbados.CommonUtils.BitManipulation;

namespace Barbados.Documents.RadixTree.Metadata
{
	internal readonly struct PrefixDescriptor
	{
		public const int BinaryLength = sizeof(uint);

		// Relative to the start of the prefix table
		private const uint _prefixLengthMask             = 0b0000_0000_0000_0000_0000_0000_0011_1111;
		private const uint _firstChildRelativeOffsetMask = 0b0011_1111_1111_1111_1111_1111_1100_0000;
		private const uint _hasValueMask                 = 0b0100_0000_0000_0000_0000_0000_0000_0000;
		private const uint _isLastChildMask              = 0b1000_0000_0000_0000_0000_0000_0000_0000;

		private const int _prefixLengthShift             = 0;
		private const int _firstChildRelativeOffsetShift = 6;

		public readonly uint Bits => _bits;

		private readonly uint _bits;

		public PrefixDescriptor(uint bits)
		{
			_bits = bits;
		}

		public PrefixDescriptor(int prefixLength, int offset, bool hasValue, bool isLastChild)
		{
			_bits = 0;
			PrefixLength = prefixLength;
			FirstChildRelativeOffset = offset;
			HasValue = hasValue;
			IsLastChild = isLastChild;
		}

		public bool HasChildren => FirstChildRelativeOffset > 0;

		public bool HasValue
		{
			readonly get => _bits.Get(_hasValueMask);
			init => _bits.Set(_hasValueMask, value);
		}

		public bool IsLastChild
		{
			readonly get => _bits.Get(_isLastChildMask);
			init => _bits.Set(_isLastChildMask, value);
		}

		public int PrefixLength
		{
			readonly get => (int)_bits.GetBits(_prefixLengthMask, _prefixLengthShift);
			init => _bits.SetBits((uint)value, _prefixLengthMask, _prefixLengthShift);
		}

		public int FirstChildRelativeOffset
		{
			readonly get => (int)_bits.GetBits(_firstChildRelativeOffsetMask, _firstChildRelativeOffsetShift);
			init => _bits.SetBits((uint)value, _firstChildRelativeOffsetMask, _firstChildRelativeOffsetShift);
		}
	}
}
