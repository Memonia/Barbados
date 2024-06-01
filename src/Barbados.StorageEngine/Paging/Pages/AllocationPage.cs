using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;

using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal sealed class AllocationPage : AbstractPage
	{
		static AllocationPage()
		{
			// For TZCNT
			Debug.Assert(Constants.AllocationBitmapPageCount >= sizeof(ulong) * 8);
			Debug.Assert(
				Constants.AllocationBitmapLength - Constants.AllocationBitmapOverheadLength >= sizeof(ulong)
			);
		}

		// See also: Allocate/Deallocate in 'PagePool'
		// Bytes in a bitmap are filled from LSD to MSD as the page handle increases, TZCNT usage depends on that

		public AllocationPage(PageHandle handle) : base(new PageHeader(handle, PageMarker.Allocation))
		{

		}

		public AllocationPage(PageBuffer buffer) : base(buffer)
		{
			var i = ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Debug.Assert(Header.Marker == PageMarker.Allocation);
		}

		public bool TryAcquireFreeHandle(PageHandle nextAvailableHandle, long currentBitmapIndex, out PageHandle handle)
		{
			var bitmap = _getBitmap();
			var ulongIndex = 0;

			// Treat the bitmap as an array of ulong to better utilize TZCNT
			for (int i = 0; i < Constants.AllocationBitmapLength / sizeof(ulong); ++i, ulongIndex += sizeof(ulong))
			{
				if (_tryAcquire(bitmap, ulongIndex, currentBitmapIndex, nextAvailableHandle, out handle))
				{
					return true;
				}
			}

			// Align byteIndex to the last ulong to complete the scan
			ulongIndex = Constants.AllocationBitmapLength - sizeof(ulong);
			return _tryAcquire(bitmap, ulongIndex, currentBitmapIndex, nextAvailableHandle, out handle);
		}

		public bool IsActive(PageHandle handle)
		{
			return (
				_getPageByte(handle) & (byte)(1 << (int)(handle.Handle % sizeof(ulong)))
			) != 0;
		}

		public void On(PageHandle handle)
		{
			_getPageByte(handle) |= (byte)(1 << (int)(handle.Handle % sizeof(ulong)));
		}

		public void Off(PageHandle handle)
		{
			_getPageByte(handle) &= (byte)~(1 << (int)(handle.Handle % sizeof(ulong)));
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			var i = WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			return PageBuffer;
		}

		private Span<byte> _getBitmap()
		{
			return PageBuffer.AsSpan().Slice(Constants.AllocationBitmapOverheadLength, Constants.AllocationBitmapLength);
		}

		private ref byte _getPageByte(PageHandle handle)
		{
			return ref _getBitmap()[(int)(handle.Handle / sizeof(ulong)) % Constants.AllocationBitmapLength];
		}

		private bool _tryAcquire(Span<byte> bitmap, int ulongIndex, long bitmapIndex, PageHandle nextAvailableHandle, out PageHandle handle)
		{
			static PageHandle _getHandle(int byteIndex, int bitIndex, long bitmapIndex)
			{
				var h = new PageHandle(
					byteIndex * 8 + bitIndex + Constants.AllocationBitmapPageCount * bitmapIndex
				);

				return h;
			}

			// Must be little endian
			var bits = BinaryPrimitives.ReadUInt64LittleEndian(bitmap[ulongIndex..]);

			// Set bits represent active pages.
			// Flipping ones allows us to count the number of them before the first free page
			var bitIndex = BitOperations.TrailingZeroCount(~bits);

			// No free pages in a current batch
			if (bitIndex == 64)
			{
				handle = default!;
				return false;
			}

			handle = _getHandle(ulongIndex, bitIndex, bitmapIndex);

			// We found an index of a zero, but it might not yet be allocated
			if (handle.Handle >= nextAvailableHandle.Handle)
			{
				handle = PageHandle.Null;
				return false;
			}

			On(handle);
			return true;
		}
	}
}
