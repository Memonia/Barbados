using System.Diagnostics;
using System.IO;

using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Paging
{
	internal partial class PagePool
	{
		/* Atrocious allocation/deallocation performance as far as both algorithm and concurrency go.
		 * Needs improvement
		 */

		public PageHandle Allocate()
		{
			bool _tryGetFreePage(out PageHandle handle)
			{
				var root = LoadPin<RootPage>(PageHandle.Root);
				var next = root.AllocationPageChainHeadHandle;

				long bitmapIndex = 0;
				while (!next.IsNull)
				{
					var bitmap = LoadPin<AllocationPage>(next);
					if (bitmap.TryAcquireFreeHandle(root.NextAvailablePageHandle, bitmapIndex, out handle))
					{
						Release(root);
						SaveRelease(bitmap);
						return true;
					}

					next = bitmap.Next;

					Release(bitmap);
					bitmapIndex += 1;
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
				var nextHandle = root.NextAvailablePageHandle;

				// Increase the file length
				RandomAccess.SetLength(
					_fileHandle,
					RandomAccess.GetLength(_fileHandle) + Constants.PageLength
				);

				// Assume one allocation page can track 128 pages, then:
				// No.	| Tracked handles	| Bitmap handle | Note
				// 1	| [1:127]			| ?				| (handle depends on how the file is initialised)
				// 2	| [128:255] 		| 128			|
				// 3	| [256:383] 		| 256			|
				//
				// Then, the number of a bitmap page for a given handle is:
				// floor(handle / 128)
				//
				// That also means that each allocation page tracks itself (see note)
				// in its first bit (except for the first one, which is never deallocated)
				//
				// Note: currently, the root handle is 1, in which case the first bit of every allocation page
				// corresponds to its own handle. If the root's handle was 0, the last bit of every previous
				// allocation page would correspond to the handle of the current allocation page. This is important,
				// because we have to mark the allocation page itself as active, so it doesn't get garbage collected.
				var freePageBitmapNum = nextHandle.Handle / Constants.AllocationBitmapPageCount;

				// Find the allocation bitmap for a new handle
				var bitmap = LoadPin<AllocationPage>(root.AllocationPageChainHeadHandle);
				for (long i = 0; i < freePageBitmapNum; ++i)
				{
					// Check if we need a new allocation page
					if (bitmap.Next.IsNull)
					{
						// Should always happen on the last iteration
						Debug.Assert(i == freePageBitmapNum - 1);

						// Create the next allocation page and append to the chain
						var nap = new AllocationPage(root.IncrementNextAvailablePageHandle());
						bitmap.Next = nap.Header.Handle;

						// The bitmap tracks itself in the first bit (see above)
						bitmap = nap;
						bitmap.On(bitmap.Header.Handle);
						break;
					}

					var next = bitmap.Next;

					Release(bitmap);
					bitmap = LoadPin<AllocationPage>(next);
				}

				// Mark the page as active
				var handle = root.IncrementNextAvailablePageHandle();
				bitmap.On(handle);

				SaveRelease(root);
				SaveRelease(bitmap);
				return handle;
			}
		}

		public void Deallocate(PageHandle handle)
		{
			lock (_sync)
			{
				var root = LoadPin<RootPage>(PageHandle.Root);
				var bitmap = LoadPin<AllocationPage>(root.AllocationPageChainHeadHandle);

				Release(root);

				var prev = PageHandle.Null;
				var freePageNum = handle.Handle / Constants.AllocationBitmapPageCount;

				// Find allocation bitmap for a given handle.
				// The page exists, because it's created during allocation of new pages
				for (long i = 0; i < freePageNum; ++i)
				{
					prev = bitmap.Header.Handle;
					var nextBitmapHandle = bitmap.Next;

					Release(bitmap);
					bitmap = LoadPin<AllocationPage>(nextBitmapHandle);
				}

				// Mark the page as free
				bitmap.Off(handle);
				SaveRelease(bitmap);
			}
		}
	}
}
