using Barbados.StorageEngine.Helpers;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal partial class SlottedPage
	{
		internal readonly struct Descriptor
		{
			public const int BinaryLength = sizeof(ulong);

			static Descriptor()
			{
				DebugHelpers.AssertSlotDescriptorLength();
			}

			private const ulong _offsetMask      = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111;
			private const ulong _lengthMask      = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_0000_0000_0000;
			private const ulong _keyLengthMask   = 0b0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_0000_0000_0000_0000_0000_0000;
			private const ulong _freeSpaceLength = 0b0000_0000_0000_0000_1111_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000;
			private const ulong _customFlagsMask = 0b0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
			private const ulong _isGarbageMask   = 0b1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

			private const int _offsetShift          = 0;
			private const int _lengthShift          = 12;
			private const int _keyLengthShift       = 24;
			private const int _freeSpaceLengthShift = 36;
			private const int _customFlagsShift     = 48;
			
			public readonly ulong Bits => _bits;

			private readonly ulong _bits;

			public Descriptor(ulong bits)
			{
				_bits = bits;
			}

			public Descriptor(ushort offset, ushort length, ushort keyLength, ushort freeSpaceLength)
			{
				_bits = 0;
				Offset = offset;
				Length = length;
				KeyLength = keyLength;
				FreeSpaceLength = freeSpaceLength;
			}

			public byte CustomFlags
			{
				readonly get => (byte)_bits.GetBits(_customFlagsMask, _customFlagsShift);
				init => _bits.SetBits(value, _customFlagsMask, _customFlagsShift);
			}

			public bool IsGarbage
			{
				readonly get => _bits.Get(_isGarbageMask);
				init => _bits.Set(_isGarbageMask, value);
			}

			public ushort Offset
			{
				readonly get => (ushort)_bits.GetBits(_offsetMask, _offsetShift);
				init => _bits.SetBits(value, _offsetMask, _offsetShift);
			}

			public ushort Length
			{
				readonly get => (ushort)_bits.GetBits(_lengthMask, _lengthShift);
				init => _bits.SetBits(value, _lengthMask, _lengthShift);
			}

			public ushort KeyLength
			{
				readonly get => (ushort)_bits.GetBits(_keyLengthMask, _keyLengthShift);
				private init => _bits.SetBits(value, _keyLengthMask, _keyLengthShift);
			}

			public ushort FreeSpaceLength
			{
				readonly get => (ushort)_bits.GetBits(_freeSpaceLength, _freeSpaceLengthShift);
				init => _bits.SetBits(value, _freeSpaceLength, _freeSpaceLengthShift);
			}

			public readonly ushort DataLength => (ushort)(Length - KeyLength - FreeSpaceLength);
		}
	}
}
