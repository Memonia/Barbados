using System;
using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Helpers;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal abstract partial class SlottedPage : AbstractPage
	{
		/* layout:
		 *  PageHeader			(AbstractPage)
		 *  SlottedPageHeader	(SlottedPage)
		 *  ExternalPayload		(derived page data)
		 *  Payload				(slots and descriptors)
		 *  
		 *  ReadBaseAndGetStartBufferOffset and WriteBaseAndGetStartBufferOffset return an offset
		 *  at which the external payload starts. Derived classes must not write or read more than
		 *  they specify by passing externalPayloadLength
		 *  
		 *  Multiple garbage descriptors might be associated with the same key
		 */

		protected SlottedPageHeader SlottedHeader => _header;
		protected IReadOnlyList<Descriptor> Descriptors => _descriptors;

		/* payload:
		 *	'SlotDescriptor'1, 'SlotDescriptor'2, -> ... <- 'Slot'1, 'Slot'2, ...
		 * Descriptors are sorted by the key stored in the slot.
		 *	
		 * slot:
		 *	 <key> <data> [free space]
		 */

		private SlottedPageHeader _header;
		private List<Descriptor> _descriptors;

		public SlottedPage(ushort externalPayloadLength, PageHeader header) : base(header)
		{
			_header = new SlottedPageHeader(
				externalPayloadLength,
				(ushort)(Constants.SlottedPagePayloadLength - externalPayloadLength)
			);

			_descriptors = [];
		}

		protected SlottedPage(PageBuffer buffer) : base(buffer)
		{
			_descriptors = [];
		}

		public int Count()
		{
			var c = 0;
			for (int i = 0; i < _descriptors.Count; ++i)
			{
				if (!_descriptors[i].IsGarbage)
				{
					c += 1;
				}
			}

			return c;
		}

		protected int GetMaxAllocatableRegionLength()
		{
			var diff = SlottedHeader.TotalFreeSpace - Descriptor.BinaryLength;
			return diff < 0 ? 0 : diff;
		}

		protected bool CanAllocate(int keyLength, int dataLength)
		{
			// The estimate is rough, because there might be big enough garbage slot,
			// reusing which would not require creating a new descriptor
			return keyLength + dataLength <= GetMaxAllocatableRegionLength();
		}

		protected Slot GetSlot(Descriptor descriptor)
		{
			var start = Constants.SlottedPageOverheadLength + _header.InternalPayloadOffset;
			return new Slot(descriptor, PageBuffer.AsSpan()[start..]);
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

		protected int DescriptorBinarySearch(ReadOnlySpan<byte> key)
		{
			int left = 0;
			int right = _descriptors.Count - 1;
			while (left <= right)
			{
				// Idea taken from .NET runtime implementation
				int m = (int)((uint)left + (uint)right >> 1);
				int c = key.SequenceCompareTo(GetSlot(_descriptors[m]).Key);

				if (c == 0)
				{
					return m;
				}

				else
				if (c > 0)
				{
					left = m + 1;
				}

				else
				{
					right = m - 1;
				}
			}

			return ~left;
		}

		protected bool TryReadFromLowest(out ReadOnlySpan<byte> key, out Span<byte> data, out byte flags)
		{
			for (int i = 0; i < _descriptors.Count; ++i)
			{
				if (_tryRead(_descriptors[i], out key, out data, out flags))
				{
					return true;
				}
			}

			key = default!;
			data = default!;
			flags = default;
			return false;
		}

		protected bool TryReadFromHighest(out ReadOnlySpan<byte> key, out Span<byte> data, out byte flags)
		{
			for (int i = _descriptors.Count - 1; i >= 0; --i)
			{
				if (_tryRead(_descriptors[i], out key, out data, out flags))
				{
					return true;
				}
			}

			key = default!;
			data = default!;
			flags = default;
			return false;
		}

		protected bool TryRead(scoped ReadOnlySpan<byte> key, out Span<byte> data, out byte flags)
		{           
			if (_tryGetActiveDescriptor(key, out _, out var descriptor))
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
			if (_tryGetActiveDescriptor(key, out var index, out var descriptor))
			{
				_descriptors[index] = descriptor with { CustomFlags = flags };
				return true;
			}

			return false;
		}

		protected bool TryRemove(ReadOnlySpan<byte> key)
		{
			if (_tryGetActiveDescriptor(key, out var index, out var descriptor))
			{
				_descriptors[index] = descriptor with { IsGarbage = true };

				// Free space of the slot has already been accounted for during slot allocation
				_header.TotalFreeSpace += (ushort)(descriptor.Length - descriptor.FreeSpaceLength);
				_header.CanCompact = true;
				return true;
			}

			return false;
		}

		protected bool TryAllocate(ReadOnlySpan<byte> key, int dataLength, out Span<byte> data)
		{
			Debug.Assert(!_tryGetActiveDescriptor(key, out _, out _));
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

			_header = new(
				HelpRead.AsUInt64(span[i..])
			);
			i += SlottedPageHeader.BinaryLength;

			Debug.Assert(i + Constants.SlottedPagePayloadLength == Constants.PageLength);

			var r = i;
			i += _header.InternalPayloadOffset;

			for (var j = 0; j < _header.SlotCount; ++j)
			{
				_descriptors.Add(
					new(HelpRead.AsUInt64(span[i..]))
				);
				i += Descriptor.BinaryLength;
			}

			return r;
		}

		protected new int WriteBaseAndGetStartBufferOffset()
		{
			var i = base.WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsUInt64(span[i..], _header.Bits);
			i += SlottedPageHeader.BinaryLength;

			Debug.Assert(i + Constants.SlottedPagePayloadLength == Constants.PageLength);

			var r = i;
			i += _header.InternalPayloadOffset;

			foreach (var descriptor in _descriptors)
			{
				HelpWrite.AsUInt64(span[i..], descriptor.Bits);
				i += Descriptor.BinaryLength;
			}

			return r;
		}

		private bool _tryRead(Descriptor descriptor, out ReadOnlySpan<byte> key, out Span<byte> data, out byte flags)
		{
			if (!descriptor.IsGarbage)
			{
				var slot = GetSlot(descriptor);
				key = slot.Key;
				data = slot.Data;
				flags = descriptor.CustomFlags;
				return true;
			}

			key = default!;
			data = default!;
			flags = default;
			return false;
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

				_writeDescriptor(key, descriptor);
				return true;
			}

			// Check if existing slots can be used
			for (int i = 0; i < _descriptors.Count; ++i)
			{
				var desc = _descriptors[i];

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
					_updateDescriptor(i, key, descriptor);
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

					_descriptors[i] = current;
					_writeDescriptor(key, descriptor);
					return true;
				}
			}

			descriptor = default!;
			return false;
		}

		private bool _tryGetActiveDescriptor(ReadOnlySpan<byte> key, out int index, out Descriptor descriptor)
		{
			index = DescriptorBinarySearch(key);
			if (index < 0)
			{
				descriptor = default;
				return false;
			}

			if (!_descriptors[index].IsGarbage)
			{
				descriptor = _descriptors[index];
				return true;
			}

			// Check the left side of the found descriptor
			for (int i = index - 1; i >= 0; --i)
			{
				descriptor = _descriptors[i];
				if (!GetSlot(descriptor).Key.SequenceEqual(key))
				{
					break;
				}

				if (!descriptor.IsGarbage)
				{
					index = i;
					return true;
				}
			}

			// Check the right side of the found descriptor
			for (int i = index + 1; i < _descriptors.Count; ++i)
			{
				descriptor = _descriptors[i];
				if (!GetSlot(descriptor).Key.SequenceEqual(key))
				{
					break;
				}

				if (!descriptor.IsGarbage)
				{
					index = i;
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

			var temp = new byte[Constants.SlottedPagePayloadLength - _header.InternalPayloadOffset];
			var tempSpan = temp.AsSpan();
			var tempDescriptors = new List<Descriptor>(_descriptors.Count);
			var count = 0;
			var slotOffset = (ushort)temp.Length;
			foreach (var descriptor in _descriptors)
			{
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

					count += 1;
					tempDescriptors.Add(desc);
				}

				else
				{
					// Length of the slot is accounted for during removal
					_header.TotalFreeSpace += Descriptor.BinaryLength;
				}
			}

			Debug.Assert(tempDescriptors.Count == count);
			_descriptors = tempDescriptors;

			var span = PageBuffer.AsSpan();
			var start = Constants.SlottedPageOverheadLength + _header.InternalPayloadOffset;
			temp.CopyTo(span[start..]);

			_header.CanCompact = false;
			_header.SlotCount = (ushort)count;
			_header.FirstSlotOffset = slotOffset;
			return true;
		}

		private void _writeDescriptor(ReadOnlySpan<byte> key, Descriptor descriptor)
		{
			var index = DescriptorBinarySearch(key);
			if (index < 0)
			{
				index = ~index;
			}

			_descriptors.Insert(index, descriptor);
		}

		private void _updateDescriptor(int index, ReadOnlySpan<byte> key, Descriptor descriptor)
		{
			_descriptors.RemoveAt(index);
			_writeDescriptor(key, descriptor);
		}
	}
}
