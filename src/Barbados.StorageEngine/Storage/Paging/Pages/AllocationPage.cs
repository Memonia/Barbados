using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Numerics;

namespace Barbados.StorageEngine.Storage.Paging.Pages
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

		// See also: Allocate/Deallocate in 'WalBuffer'
		// Bytes in a bitmap are filled from LSD to MSD as the page handle increases, TZCNT usage depends on that

		public AllocationPage(PageHandle handle) : base(new PageHeader(handle, PageMarker.Allocation))
		{

		}

		public AllocationPage(PageBuffer buffer) : base(buffer)
		{
			ReadBaseAndGetStartBufferOffset();
			Debug.Assert(Header.Marker == PageMarker.Allocation);
		}

		public bool TryAcquireFreeHandle(PageHandle nextAvailableHandle, long currentBitmapIndex, out PageHandle handle)
		{
			bool _tryAcquire(Span<byte> bitmap, int ulongIndex, out PageHandle handle)
			{
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

				handle = new PageHandle(
					ulongIndex * 8 + bitIndex + Constants.AllocationBitmapPageCount * currentBitmapIndex
				);

				return true;
			}

			var bitmap = _getBitmap();
			var acquired = false;
			var ulongIndex = 0;

			// Treat the bitmap as an array of ulong to better utilize TZCNT
			handle = PageHandle.Null;
			for (int i = 0; i < Constants.AllocationBitmapLength / sizeof(ulong); ++i, ulongIndex += sizeof(ulong))
			{
				acquired = _tryAcquire(bitmap, ulongIndex, out handle);
				if (acquired)
				{
					break;
				}
			}

			if (!acquired)
			{
				// Align to the last ulong to complete the scan
				ulongIndex = Constants.AllocationBitmapLength - sizeof(ulong);
				acquired = _tryAcquire(bitmap, ulongIndex, out handle);
			}

			if (acquired && handle.Handle < nextAvailableHandle.Handle)
			{
				On(handle);
				return true;
			}

			return false;
		}

		public bool IsActive(PageHandle handle)
		{
			return (
				_getByteRef(handle) & (byte)(1 << (int)(handle.Handle % sizeof(ulong)))
			) != 0;
		}

		public void On(PageHandle handle)
		{
			_getByteRef(handle) |= (byte)(1 << (int)(handle.Handle % sizeof(ulong)));
		}

		public void Off(PageHandle handle)
		{
			_getByteRef(handle) &= (byte)~(1 << (int)(handle.Handle % sizeof(ulong)));
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			WriteBaseAndGetStartBufferOffset();
			return PageBuffer;
		}

		private Span<byte> _getBitmap()
		{
			return PageBuffer.AsSpan().Slice(Constants.AllocationBitmapOverheadLength, Constants.AllocationBitmapLength);
		}

		private ref byte _getByteRef(PageHandle handle)
		{
			return ref _getBitmap()[(int)(handle.Handle / sizeof(ulong)) % Constants.AllocationBitmapLength];
		}
	}
}
