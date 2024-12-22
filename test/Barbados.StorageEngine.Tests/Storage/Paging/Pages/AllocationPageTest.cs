using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Tests.Storage.Paging.Pages
{
	public sealed class AllocationPageTest
	{
		public sealed class On
		{
			[Test]
			public void WasOffSetOn_IsActiveTrue()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.On(handle);

				Assert.That(page.IsActive(handle), Is.True);
			}

			[Test]
			public void WasOnSetOn_IsActiveTrue()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.On(handle);
				page.On(handle);

				Assert.That(page.IsActive(handle), Is.True);
			}
		}

		public sealed class Off
		{
			[Test]
			public void SetOff_IsActiveFalse()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.Off(handle);

				Assert.That(page.IsActive(handle), Is.False);
			}

			[Test]
			public void WasOnSetOff_IsActiveFalse()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.On(handle);
				page.Off(handle);

				Assert.That(page.IsActive(handle), Is.False);
			}

			[Test]
			public void WasOffSetOff_IsActiveFalse()
			{
				var page = new AllocationPage(new PageHandle(0));
				var handle = new PageHandle(1);

				page.Off(handle);
				page.Off(handle);

				Assert.That(page.IsActive(handle), Is.False);
			}
		}

		public sealed class TryAcquireFreeHandle
		{
			[Test]
			// First page of the bitmap
			[TestCase(0, 0)]
			[TestCase(1, Constants.AllocationBitmapPageCount)]
			// Last page of the bitmap
			[TestCase(0, Constants.AllocationBitmapPageCount - 1)]
			[TestCase(1, Constants.AllocationBitmapPageCount * 2 - 1)]
			// Something in the middle
			[TestCase(0, Constants.AllocationBitmapPageCount / 4)]
			[TestCase(0, Constants.AllocationBitmapPageCount / 8)]
			[TestCase(1, Constants.AllocationBitmapPageCount / 4 + Constants.AllocationBitmapPageCount)]
			[TestCase(1, Constants.AllocationBitmapPageCount / 8 + Constants.AllocationBitmapPageCount)]
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

				Assert.Multiple(() =>
				{
					Assert.That(r, Is.True);
					Assert.That(handle.Handle, Is.EqualTo(pageHandleIndex));
				});
			}

			[Test]
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

				Assert.That(r, Is.False);
			}
		}
	}
}
