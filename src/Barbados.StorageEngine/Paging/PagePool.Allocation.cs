using System.IO;

using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Paging
{
	internal partial class PagePool
	{
		private static PageHandle _getAllocationPageHandle(PageHandle handle, RootPage root)
		{
			// See the comment below
			var index = handle.Handle / Constants.AllocationBitmapPageCount;
			var allocHandle = new PageHandle(index * Constants.AllocationBitmapPageCount);
			if (allocHandle.Handle == 0)
			{
				return root.FirstAllocationPageHandle;
			}

			return allocHandle;
		}

		// Assume one allocation page can track 128 pages, then:
		// No.	| Tracked handles	| Bitmap handle | Note
		// 1	| [0:127]			| ?				| (handle depends on how the file is initialised)
		// 2	| [128:255] 		| 128			|
		// 3	| [256:383] 		| 256			|
		//
		// Then, the index of the allocation page in a file for a given handle is:
		// floor(handle / 128)
		// 
		// And so the handle of the allocation page is:
		// floor(handle / 128) * 128
		// 
		// That also means that each allocation page tracks itself (see note)
		// in its first bit (except for the first one, which is never deallocated)
		//
		// Note: currently, the root handle is 1, in which case the first bit of every allocation page
		// corresponds to its own handle. If the root's handle was 0, the last bit of every previous
		// allocation page would correspond to the handle of the current allocation page. This is important,
		// because we have to mark the allocation page itself as active, so it doesn't get garbage collected.

		public PageHandle Allocate()
		{
			bool _tryGetFreePage(out PageHandle handle)
			{
				var root = LoadPin<RootPage>(PageHandle.Root);
				var bitmap = LoadPin<AllocationPage>(root.FirstAllocationPageHandle);
				var bitmapIndex = 0;

				// Try the first page
				if (bitmap.TryAcquireFreeHandle(root.NextAvailablePageHandle, bitmapIndex, out handle))
				{
					Release(root);
					SaveRelease(bitmap);
					return true;
				}

				Release(bitmap);

				// Try the rest (see the comment at the top)
				bitmapIndex += 1;
				var bitmapHandle = new PageHandle(Constants.AllocationBitmapPageCount);
				while (bitmapHandle.Handle < root.LastAllocationPageHandle.Handle)
				{
					bitmap = LoadPin<AllocationPage>(bitmapHandle);
					if (bitmap.TryAcquireFreeHandle(root.NextAvailablePageHandle, bitmapIndex, out handle))
					{
						Release(root);
						SaveRelease(bitmap);
						return true;
					}

					Release(bitmap);
					bitmapIndex += 1;
					bitmapHandle = new PageHandle(bitmapHandle.Handle + Constants.AllocationBitmapPageCount);
				}

				Release(root);
				handle = PageHandle.Null;
				return false;
			}

			lock (_sync)
			{
				// Reuse already allocated pages if possible
				if (_tryGetFreePage(out var fh))
				{
					return fh;
				}

				var root = LoadPin<RootPage>(PageHandle.Root);
				var bitmapHandle = _getAllocationPageHandle(root.NextAvailablePageHandle, root);

				// Check if we need a new allocation page
				if (bitmapHandle.Handle > root.LastAllocationPageHandle.Handle)
				{
					bitmapHandle = root.IncrementNextAvailablePageHandle();
					root.LastAllocationPageHandle = bitmapHandle;

					// Increase the file length for both the new allocation page and a newly allocated page
					RandomAccess.SetLength(
						_fileHandle, RandomAccess.GetLength(_fileHandle) + Constants.PageLength * 2
					);

					// See the comment at the top
					var nap = new AllocationPage(bitmapHandle);
					nap.On(bitmapHandle);
					Save(root);
					Save(nap);
				}

				else
				{
					// Increase the file length for a newly allocated page
					RandomAccess.SetLength(
						_fileHandle, RandomAccess.GetLength(_fileHandle) + Constants.PageLength
					);
				}

				var handle = root.IncrementNextAvailablePageHandle();

				// Mark newly allocated page as active
				var bitmap = LoadPin<AllocationPage>(bitmapHandle);
				bitmap.On(handle);

				SaveRelease(root);
				SaveRelease(bitmap);
				return handle;
			}
		}

		public void Deallocate(PageHandle handle)
		{
			DEBUG_ThrowUnallocatedHandle(handle);
			lock (_sync)
			{
				var root = LoadPin<RootPage>(PageHandle.Root);
				var bitmapHandle = _getAllocationPageHandle(handle, root);
				var bitmap = LoadPin<AllocationPage>(bitmapHandle);

				// Mark the page as free
				bitmap.Off(handle);

				Release(root);
				SaveRelease(bitmap);
			}
		}
	}
}
