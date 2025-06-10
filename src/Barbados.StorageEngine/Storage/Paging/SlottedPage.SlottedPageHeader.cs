using System.Diagnostics;

using Barbados.CommonUtils.BitManipulation;

namespace Barbados.StorageEngine.Storage.Paging
{
	internal partial class SlottedPage
	{
		public struct SlottedPageHeader
		{
			public const int BinaryLength = sizeof(ulong);

			static SlottedPageHeader()
			{
				Debug.Assert(Constants.PageLength == 4096);
			}   
			
			/* The number of bits taken here depends on the page length. Currently it's 4096 bytes
			 */

			private const ulong _internalPayloadOffsetMask = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111;
			private const ulong _totalFreeSpaceMask        = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_0000_0000_0000;
			private const ulong _slotRegionStartMask       = 0b0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_0000_0000_0000_0000_0000_0000;
			private const ulong _slotCountMask             = 0b0000_0000_0000_0000_0001_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000;
			private const ulong _canCompactMask            = 0b1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

			private const int _internalPayloadOffsetShift = 0;
			private const int _totalFreeSpaceShift        = 12;
			private const int _slotRegionStartShift       = 24;
			private const int _slotCountShift             = 36;

			public readonly ulong Bits => _bits;

			private ulong _bits;

			public SlottedPageHeader(ulong bits)
			{
				_bits = bits;
			}

			public SlottedPageHeader(ushort internalRegionOffset, ushort freeSpaceLength)
			{
				_bits = 0;
				TotalFreeSpace = freeSpaceLength;
				FirstSlotOffset = freeSpaceLength;
				InternalRegionOffset = internalRegionOffset;
			}

			public bool CanCompact
			{
				readonly get => _bits.Get(_canCompactMask);
				set => _bits.Set(_canCompactMask, value);
			}

			public ushort SlotCount
			{
				readonly get => (ushort)_bits.GetBits(_slotCountMask, _slotCountShift);
				set => _bits.SetBits(value, _slotCountMask, _slotCountShift);
			}

			public ushort TotalFreeSpace
			{
				readonly get => (ushort)_bits.GetBits(_totalFreeSpaceMask, _totalFreeSpaceShift);
				set => _bits.SetBits(value, _totalFreeSpaceMask, _totalFreeSpaceShift);
			}

			public ushort FirstSlotOffset
			{
				readonly get => (ushort)_bits.GetBits(_slotRegionStartMask, _slotRegionStartShift);
				set => _bits.SetBits(value, _slotRegionStartMask, _slotRegionStartShift);
			}

			public ushort InternalRegionOffset
			{
				readonly get => (ushort)_bits.GetBits(_internalPayloadOffsetMask, _internalPayloadOffsetShift);
				private set => _bits.SetBits(value, _internalPayloadOffsetMask, _internalPayloadOffsetShift);
			}

			public readonly double UnoccupiedPercentage => TotalFreeSpace / (double)(PayloadLength - InternalRegionOffset);

			public readonly ushort MaxPossibleAllocatableRegionLength
			{
				get
				{
					var diff = TotalFreeSpace - Descriptor.BinaryLength;
					return (ushort)(diff < 0 ? 0 : diff);
				}
			}

			public readonly ushort LengthBetweenLastDescriptorAndFirstSlot => (ushort)(FirstSlotOffset - SlotCount * Descriptor.BinaryLength);
		}
	}
}
