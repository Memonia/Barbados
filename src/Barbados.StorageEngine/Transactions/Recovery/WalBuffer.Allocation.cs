using Barbados.StorageEngine.Storage;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Transactions.Recovery
{
	internal partial class WalBuffer
	{
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

		public static ulong AllocateRootAndGetMagic(IStorageWrapper storage)
		{
			// Init root page
			var root = new RootPage();
			var apageHandle = root.IncrementNextAvailablePageHandle();
			var cpageHandle = root.IncrementNextAvailablePageHandle();
			var ipageHandle = root.IncrementNextAvailablePageHandle();

			// Init the allocation bitmap
			var apage = new AllocationPage(apageHandle);
			root.FirstAllocationPageHandle = apage.Header.Handle;
			root.LastAllocationPageHandle = apage.Header.Handle;

			// Init the meta collection
			var cpage = new CollectionPage(cpageHandle);
			var ipage = new BTreeRootPage(ipageHandle);

			root.MetaCollectionPageHandle = cpage.Header.Handle;
			root.MetaCollectionNameIndexRootPageHandle = ipage.Header.Handle;

			// Mark created pages as active
			apage.On(root.Header.Handle);
			apage.On(apage.Header.Handle);
			apage.On(cpage.Header.Handle);
			apage.On(ipage.Header.Handle);

			// Mark null handle as active so it can't be allocated
			apage.On(PageHandle.Null);

			AbstractPage.WriteChecksum(root.UpdateAndGetBuffer());
			AbstractPage.WriteChecksum(apage.UpdateAndGetBuffer());
			AbstractPage.WriteChecksum(cpage.UpdateAndGetBuffer());
			AbstractPage.WriteChecksum(ipage.UpdateAndGetBuffer());

			storage.Write(root.Header.Handle.GetAddress(), root.UpdateAndGetBuffer().AsSpan());
			storage.Write(apage.Header.Handle.GetAddress(), apage.UpdateAndGetBuffer().AsSpan());
			storage.Write(cpage.Header.Handle.GetAddress(), cpage.UpdateAndGetBuffer().AsSpan());
			storage.Write(ipage.Header.Handle.GetAddress(), ipage.UpdateAndGetBuffer().AsSpan());
			storage.Persist();
			return root.FileMagic;
		}

		private static PageHandle _getAllocationPageHandle(PageHandle handle, RootPage root)
		{
			var index = handle.Handle / Constants.AllocationBitmapPageCount;
			var allocHandle = new PageHandle(index * Constants.AllocationBitmapPageCount);
			if (allocHandle.Handle == 0)
			{
				return root.FirstAllocationPageHandle;
			}

			return allocHandle;
		}

		public PageHandle Allocate(Snapshot snapshot)
		{
			bool _tryGetFreePageHandle(out PageHandle handle)
			{
				var root = LoadPin<RootPage>(_allocatorSnapshot, PageHandle.Root);
				var bitmap = LoadPin<AllocationPage>(_allocatorSnapshot, root.FirstAllocationPageHandle);
				var bitmapIndex = 0;

				// Try the first page
				if (bitmap.TryAcquireFreeHandle(root.NextAvailablePageHandle, bitmapIndex, out handle))
				{
					Save(_allocatorSnapshot, bitmap);
					return true;
				}

				// Try the rest (see the comment at the top)
				bitmapIndex += 1;
				var bitmapHandle = new PageHandle(Constants.AllocationBitmapPageCount);
				while (bitmapHandle.Handle < root.LastAllocationPageHandle.Handle)
				{
					bitmap = LoadPin<AllocationPage>(_allocatorSnapshot, bitmapHandle);
					if (bitmap.TryAcquireFreeHandle(root.NextAvailablePageHandle, bitmapIndex, out handle))
					{
						Save(_allocatorSnapshot, bitmap);
						return true;
					}

					bitmapIndex += 1;
					bitmapHandle = new PageHandle(bitmapHandle.Handle + Constants.AllocationBitmapPageCount);
				}

				handle = PageHandle.Null;
				return false;
			}

			var info = _getTransactionInfo(snapshot);
			lock (_allocatorSync)
			{
				// Reuse already allocated pages if possible
				if (_tryGetFreePageHandle(out var fh))
				{
					info.Allocated.Add(fh);
					info.Deallocated.Remove(fh);
					return fh;
				}

				var root = LoadPin<RootPage>(_allocatorSnapshot, PageHandle.Root);
				var bitmapHandle = _getAllocationPageHandle(root.NextAvailablePageHandle, root);

				// Check if we need a new allocation page
				if (bitmapHandle.Handle > root.LastAllocationPageHandle.Handle)
				{
					bitmapHandle = root.IncrementNextAvailablePageHandle();
					var nap = new AllocationPage(bitmapHandle);
					nap.On(bitmapHandle);
					root.LastAllocationPageHandle = bitmapHandle;

					Save(_allocatorSnapshot, nap);
					Save(_allocatorSnapshot, root);
				}

				var handle = root.IncrementNextAvailablePageHandle();

				// Mark newly allocated page as active
				var bitmap = LoadPin<AllocationPage>(_allocatorSnapshot, bitmapHandle);
				bitmap.On(handle);

				Save(_allocatorSnapshot, root);
				Save(_allocatorSnapshot, bitmap);

				info.Allocated.Add(handle);
				info.Deallocated.Remove(handle);
				return handle;
			}
		}

		public void Deallocate(Snapshot snapshot, PageHandle handle)
		{
			var info = _getTransactionInfo(snapshot);
			lock (_allocatorSync)
			{
				var root = LoadPin<RootPage>(_allocatorSnapshot, PageHandle.Root);
				var bitmapHandle = _getAllocationPageHandle(handle, root);
				var bitmap = LoadPin<AllocationPage>(_allocatorSnapshot, bitmapHandle);

				// Mark the page as free
				bitmap.Off(handle);
				Save(_allocatorSnapshot, bitmap);

				info.Allocated.Remove(handle);
				info.Deallocated.Add(handle);
			}
		}

		private void _saveAllocationChainUpdates(WalTransactionInfo info)
		{
			lock (_allocatorSync)
			{
				var maxh = PageHandle.Null;
				var root = LoadPin<RootPage>(_allocatorSnapshot, PageHandle.Root);
				foreach (var handle in info.Allocated)
				{
					if (handle.Handle > maxh.Handle)
					{
						maxh = handle;
					}

					var ah = _getAllocationPageHandle(handle, root);
					var apage = LoadPin<AllocationPage>(_allocatorSnapshot, ah);
					apage.On(handle);
					Save(info.Snapshot, apage);
				}

				foreach (var handle in info.Deallocated)
				{
					var ah = _getAllocationPageHandle(handle, root);
					var apage = LoadPin<AllocationPage>(_allocatorSnapshot, ah);
					apage.Off(handle);
					Save(info.Snapshot, apage);
				}

				if (maxh.Handle >= root.NextAvailablePageHandle.Handle)
				{
					root.SetNextAvailablePageHandle(new(maxh.Handle + 1));
				}

				Save(info.Snapshot, root);
			}
		}
	}
}
