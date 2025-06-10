using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barbados.StorageEngine.Storage.Paging
{
	internal abstract partial class SlottedPage : AbstractPage
	{
		public new const int ThisHeaderLength = SlottedPageHeader.BinaryLength;
		public new const int HeaderLength = AbstractPage.HeaderLength + ThisHeaderLength;
		public new const int PayloadLength = AbstractPage.PayloadLength - ThisHeaderLength;
		public const int WorstCaseFixedLengthOverheadPerEntry = Descriptor.BinaryLength;

		/* layout:
		 *  base data           (AbstractPage)
		 *  SlottedPageHeader   (SlottedPage)
		 *  ExternalPayload	    (derived page data)
		 *  Payload	            (slots and descriptors)
		 *  
		 *  ReadBaseAndGetStartBufferOffset and WriteBaseAndGetStartBufferOffset return an offset
		 *  at which the external payload starts. Derived classes must not write or read more than
		 *  they specify by passing externalPayloadLength
		 *  
		 *  Multiple garbage descriptors might be associated with the same key
		 */

		protected SlottedPageHeader SlottedHeader => _header;
		protected IActiveDescriptorList ActiveDescriptors => _descriptorList;

		/* payload:
		 *	'SlotDescriptor'1, 'SlotDescriptor'2, -> ... <- 'Slot'1, 'Slot'2, ...
		 * Descriptors are sorted by the key stored in the slot.
		 *	
		 * slot:
		 *	 <key> <data> [free space]
		 */

		private SlottedPageHeader _header;
		private readonly DescriptorList _descriptorList;
		private int _writableRegionOffset => HeaderLength + _header.InternalRegionOffset;

		public SlottedPage(ushort externalPayloadLength, PageHeader header) : base(header)
		{
			_header = new SlottedPageHeader(
				externalPayloadLength, (ushort)(PayloadLength - externalPayloadLength)
			);

			_descriptorList = new(GetSlot);
		}

		protected SlottedPage(PageBuffer buffer) : base(buffer)
		{
			/* Initialisation of a page in this case happens in 'ReadBaseAndGetStartBufferOffset' method
			 */

			_descriptorList = new(GetSlot);
		}

		protected bool CanAllocate(int keyLength, int dataLength)
		{
			// The estimate is rough, because there might be big enough garbage slot,
			// reusing which would not require creating a new descriptor. The estimate works
			// because we're always trying to compact the page before giving up on insertion
			return keyLength + dataLength <= SlottedHeader.MaxPossibleAllocatableRegionLength;
		}

		protected Slot GetSlot(Descriptor descriptor)
		{
			return new Slot(descriptor, PageBuffer.AsSpan()[_writableRegionOffset..]);
		}

		protected KeyEnumerator GetKeyEnumerator()
		{
			return new KeyEnumerator(new(this));
		}

		protected DataEnumerator GetDataEnumerator()
		{
			return new DataEnumerator(new(this));
		}

		protected SlotEnumerator GetSlotEnumerator()
		{
			return new SlotEnumerator(this);
		}

		protected bool TryRead(scoped ReadOnlySpan<byte> key, out Span<byte> data, out byte flags)
		{
			if (_descriptorList.TryGetActive(key, out var descriptor))
			{
				var slot = GetSlot(descriptor);
				data = slot.Data;
				flags = descriptor.CustomFlags;
				return true;
			}

			data = default!;
			flags = default;
			return false;
		}

		protected bool TryWrite(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data)
		{
			if (TryAllocate(key, data.Length, out var writable))
			{
				data.CopyTo(writable);
				return true;
			}

			return false;
		}

		protected bool TrySetFlags(ReadOnlySpan<byte> key, byte flags)
		{
			if (_descriptorList.TryGetActive(key, out var index, out var descriptor))
			{
				_descriptorList.Update(index, key, descriptor with { CustomFlags = flags });
				return true;
			}

			return false;
		}

		protected bool TryRemove(ReadOnlySpan<byte> key)
		{
			if (_descriptorList.TryGetActive(key, out var index, out var descriptor))
			{
				_descriptorList.Update(index, key, descriptor with { IsGarbage = true });

				// Free space of the slot has already been accounted for during slot allocation
				_header.TotalFreeSpace += (ushort)(descriptor.Length - descriptor.FreeSpaceLength);
				_header.CanCompact = true;
				return true;
			}

			return false;
		}

		protected bool TryAllocate(ReadOnlySpan<byte> key, int dataLength, out Span<byte> data)
		{
			Debug.Assert(!_descriptorList.TryGetActive(key, out _));
			if (dataLength > Constants.PageLength)
			{
				data = default!;
				return false;
			}

			if (
				_tryAllocate(key, (ushort)dataLength, out var descriptor) || (
					_tryCompact() && _tryAllocate(key, (ushort)dataLength, out descriptor)
				)
			)
			{
				var slot = GetSlot(descriptor);
				key.CopyTo(slot.Key);
				data = slot.Data;

				Debug.Assert(dataLength == data.Length);
				Debug.Assert(key.Length == descriptor.KeyLength);
				return true;
			}

			data = default!;
			return false;
		}

		protected new int ReadBaseAndGetStartBufferOffset()
		{
			var i = base.ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			_header = new(HelpRead.AsUInt64(span[i..]));
			i += SlottedPageHeader.BinaryLength;

			Debug.Assert(i + PayloadLength == Constants.PageLength);

			var r = i;
			i += _header.InternalRegionOffset;

			for (var j = 0; j < _header.SlotCount; ++j)
			{
				_descriptorList.Add(new(HelpRead.AsUInt64(span[i..])));
				i += Descriptor.BinaryLength;
			}

			// TOOD: REMOVE
			//_writableRegionOffset = HeaderLength + _header.InternalRegionOffset;
			return r;
		}

		protected new int WriteBaseAndGetStartBufferOffset()
		{
			var i = base.WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsUInt64(span[i..], _header.Bits);
			i += SlottedPageHeader.BinaryLength;

			Debug.Assert(i + PayloadLength == Constants.PageLength);

			var r = i;
			i += _header.InternalRegionOffset;

			for (int index = 0; index < _descriptorList.Count; ++index)
			{
				HelpWrite.AsUInt64(span[i..], _descriptorList.Get(index).Bits);
				i += Descriptor.BinaryLength;
			}

			return r;
		}

		private bool _tryAllocate(ReadOnlySpan<byte> key, ushort dataLength, out Descriptor descriptor)
		{
			var payloadLength = (ushort)(key.Length + dataLength);
			var payloadWithDescriptorLength = (ushort)(payloadLength + Descriptor.BinaryLength);

			// Abort if there is not enough free space
			if (payloadWithDescriptorLength > _header.TotalFreeSpace)
			{
				descriptor = default!;
				return false;
			}

			// Try to allocate a slot in the free region
			if (_header.LengthBetweenLastDescriptorAndFirstSlot >= payloadWithDescriptorLength)
			{
				_header.SlotCount += 1;
				_header.TotalFreeSpace -= payloadWithDescriptorLength;
				_header.FirstSlotOffset -= payloadLength;

				descriptor = new Descriptor(
					offset: _header.FirstSlotOffset,
					length: payloadLength,
					keyLength: (ushort)key.Length,
					freeSpaceLength: 0
				);

				_descriptorList.Add(key, descriptor);
				return true;
			}

			// Check if existing slots can be used
			for (int i = 0; i < _descriptorList.Count; ++i)
			{
				var desc = _descriptorList.Get(i);

				// If it's garbage, is it big enough?
				if (desc.IsGarbage && desc.Length >= payloadLength)
				{
					descriptor = new Descriptor(
						offset: desc.Offset,
						length: desc.Length,
						keyLength: (ushort)key.Length,
						freeSpaceLength: (ushort)(desc.Length - payloadLength)
					);

					if (descriptor.FreeSpaceLength != 0)
					{
						_header.CanCompact = true;
					}

					_header.TotalFreeSpace -= payloadLength;
					_descriptorList.Update(i, key, descriptor);
					return true;
				}

				// If it's not garbage, can the free space in the slot fit the new slot?
				if (!desc.IsGarbage && desc.FreeSpaceLength >= payloadLength)
				{
					// Allocating a slot in the free space of an existing slot requires creating a descriptor
					if (_header.LengthBetweenLastDescriptorAndFirstSlot < Descriptor.BinaryLength)
					{
						continue;
					}

					var current = desc with
					{
						Length = (ushort)(desc.Length - desc.FreeSpaceLength),
						FreeSpaceLength = 0
					};

					descriptor = new Descriptor(
						offset: (ushort)(desc.Offset + current.Length),
						length: desc.FreeSpaceLength,
						keyLength: (ushort)key.Length,
						freeSpaceLength: (ushort)(desc.FreeSpaceLength - payloadLength)
					);

					if (descriptor.FreeSpaceLength != 0)
					{
						_header.CanCompact = true;
					}

					_header.SlotCount += 1;
					_header.TotalFreeSpace -= payloadWithDescriptorLength;

					_descriptorList.Update(i, current);
					_descriptorList.Add(key, descriptor);
					return true;
				}
			}

			descriptor = default!;
			return false;
		}

		private bool _tryCompact()
		{
			/* Removes external fragmentation between slots and internal fragmentation within slots
			 */

			if (!_header.CanCompact)
			{
				return false;
			}

			var temp = new byte[PayloadLength - _header.InternalRegionOffset];
			var tempSpan = temp.AsSpan();
			var tempDescriptors = new List<Descriptor>();
			var slotOffset = (ushort)temp.Length;
			for (int i = 0; i < _descriptorList.Count; ++i)
			{
				var descriptor = _descriptorList.Get(i);
				if (!descriptor.IsGarbage)
				{
					slotOffset = (ushort)(slotOffset - descriptor.Length + descriptor.FreeSpaceLength);
					var desc = descriptor with
					{
						Offset = slotOffset,
						Length = (ushort)(descriptor.Length - descriptor.FreeSpaceLength),
						FreeSpaceLength = 0
					};

					var slot = GetSlot(descriptor);
					slot.Key.CopyTo(tempSpan[slotOffset..]);
					slot.Data.CopyTo(tempSpan[(slotOffset + slot.Key.Length)..]);

					tempDescriptors.Add(desc);
				}

				else
				{
					// Length of the slot is accounted for during removal
					_header.TotalFreeSpace += Descriptor.BinaryLength;
				}
			}

			var span = PageBuffer.AsSpan();
			temp.CopyTo(span[_writableRegionOffset..]);

			// Descriptor list is updated after the physical buffer has been modified,
			// so that descriptor slots can be retrieved properly from inside the list

			_descriptorList.Clear();
			foreach (var descriptor in tempDescriptors)
			{
				_descriptorList.Add(descriptor);
			}

			_header.CanCompact = false;
			_header.SlotCount = (ushort)tempDescriptors.Count;
			_header.FirstSlotOffset = slotOffset;
			return true;
		}
	}
}
