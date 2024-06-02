using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Tests.Paging.Pages
{
	public class AllocationPageTest
	{
		public class On
		{
			[Fact]
			public void WasOffSetOn_IsActiveTrue()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.On(handle);

				Assert.True(page.IsActive(handle));
			}

			[Fact]
			public void WasOnSetOn_IsActiveTrue()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.On(handle);
				page.On(handle);

				Assert.True(page.IsActive(handle));
			}
		}

		public class Off
		{
			[Fact]
			public void SetOff_IsActiveFalse()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.Off(handle);

				Assert.False(page.IsActive(handle));
			}

			[Fact]
			public void WasOnSetOff_IsActiveFalse()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.On(handle);
				page.Off(handle);

				Assert.False(page.IsActive(handle));
			}

			[Fact]
			public void WasOffSetOff_IsActiveFalse()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.Off(handle);
				page.Off(handle);

				Assert.False(page.IsActive(handle));
			}
		}

		public class TryAcquireFreeHandle
		{
			[Theory]
			// First page of the bitmap
			[InlineData(0, 0)]
			[InlineData(1, Constants.AllocationBitmapPageCount)]
			// Last page of the bitmap
			[InlineData(0, Constants.AllocationBitmapPageCount - 1)]
			[InlineData(1, Constants.AllocationBitmapPageCount * 2 - 1)]
			// Something in the middle
			[InlineData(0, Constants.AllocationBitmapPageCount / 4)]
			[InlineData(0, Constants.AllocationBitmapPageCount / 8)]
			[InlineData(1, Constants.AllocationBitmapPageCount / 4 + Constants.AllocationBitmapPageCount)]
			[InlineData(1, Constants.AllocationBitmapPageCount / 8 + Constants.AllocationBitmapPageCount)]
			public void FreeHandleAvailable_Success(long bitmapIndex, long pageHandleIndex)
			{
				var page = new AllocationPage(new PageHandle(0));

				// Activate all handles
				for (long i = 0; i < Constants.AllocationBitmapPageCount; ++i)
				{
					page.On(new(i));
				}

				// Deactivate the tested one
				page.Off(new(pageHandleIndex));

				var r = page.TryAcquireFreeHandle(
					new(Constants.AllocationBitmapPageCount * (bitmapIndex + 1)), bitmapIndex, out var handle
				);

				Assert.True(r);
				Assert.Equal(pageHandleIndex, handle.Handle);
			}

			[Fact]
			public void FreeHandleUnavailable_Failure()
			{
				var nextHandle = new PageHandle(Constants.AllocationBitmapPageCount);
				var index = 0;
				var page = new AllocationPage(new PageHandle(0));

				// Activate all handles
				for (int i = 0; i < Constants.AllocationBitmapPageCount; ++i)
				{
					page.On(new(i));
				}

				var r = page.TryAcquireFreeHandle(nextHandle, index, out _);

				Assert.False(r);
			}
		}
	}
}
