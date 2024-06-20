using System;
using System.Diagnostics;
using System.IO;

using Barbados.StorageEngine.Caching;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

using Microsoft.Win32.SafeHandles;

namespace Barbados.StorageEngine.Paging
{
	internal sealed partial class PagePool
	{
		public static void AllocateRoot(SafeFileHandle fileHandle)
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

			_writePageBuffer(fileHandle, root);
			_writePageBuffer(fileHandle, apage);
			_writePageBuffer(fileHandle, cpage);
			_writePageBuffer(fileHandle, ipage);

			RandomAccess.FlushToDisk(fileHandle);
		}

		private static void _writePageBuffer(SafeFileHandle fileHandle, AbstractPage page)
		{
			var buffer = page.UpdateAndGetBuffer();
			RandomAccess.Write(fileHandle, buffer.AsSpan(), page.Header.Handle.GetAddress());
		}

		private static PageBuffer _readPageBuffer(SafeFileHandle fileHandle, PageHandle handle)
		{
			var buffer = new PageBuffer();
			var r = RandomAccess.Read(fileHandle, buffer.AsSpan(), handle.GetAddress());

			if (r < Constants.PageLength)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.UnexpectedEndOfFile, "Could not read a page from the database file"
				);
			}

			if (r > Constants.PageLength)
			{
				throw new BarbadosException(BarbadosExceptionCode.InternalError);
			}

			return buffer;
		}

		private readonly object _sync;
		private readonly SafeFileHandle _fileHandle;
		private readonly ICache<PageHandle, AbstractPage> _cache;

		public PagePool(SafeFileHandle fileHandle, CacheFactory cacheFactory)
		{
			_sync = new();
			_fileHandle = fileHandle;
			_cache = cacheFactory.GetCache<PageHandle, AbstractPage>();
			_cache.OnDirtyValueEviction += (handle, page) => _writePageBuffer(page);

			var buffer = _readPageBuffer(_fileHandle, PageHandle.Root);
			RootPage.ThrowDatabaseDoesNotExist(buffer);
			RootPage.ThrowDatabaseVersionMismatch(buffer);
		}

		public bool IsPageType(PageHandle handle, PageMarker marker)
		{
			if (_cache.TryGetWithPin(handle, out var page))
			{
				var result = page.Header.Marker == marker;
				Release(page);
				return result;
			}

			var buffer = _readPageBuffer(handle);
			return AbstractPage.GetPageMarker(buffer) == marker;
		}

		public void Flush()
		{
			lock (_sync)
			{
				foreach (var k in _cache.Keys)
				{
					if (!_cache.TryGet(k, out var page))
					{
						Debug.Assert(false);
					}

					_writePageBuffer(page);
				}

				RandomAccess.FlushToDisk(_fileHandle);
			}
		}

		public T LoadPin<T>(PageHandle handle) where T : AbstractPage
		{
			if (_cache.TryGetWithPin(handle, out var page))
			{
				return (T)page;
			}

			lock (_sync)
			{
				if (_cache.TryGetWithPin(handle, out page))
				{
					return (T)page;
				}

				var buffer = _readPageBuffer(handle);
				if (typeof(T) == typeof(BTreePage))
				{
					var marker = AbstractPage.GetPageMarker(buffer);
					if (marker == PageMarker.BTreeRoot)
					{
						page = new BTreeRootPage(buffer);
					}

					else 
					if (marker == PageMarker.Collection)
					{
						page = new CollectionPage(buffer);
					}

					else
					{
						page = new BTreePage(buffer);
					}
				}

				else
				{
					page = typeof(T) switch
					{
						Type rpage when rpage == typeof(RootPage) => new RootPage(buffer),
						Type opage when opage == typeof(ObjectPage) => new ObjectPage(buffer),
						Type apage when apage == typeof(AllocationPage) => new AllocationPage(buffer),
						Type cpage when cpage == typeof(CollectionPage) => new CollectionPage(buffer),
						Type brpage when brpage == typeof(BTreeRootPage) => new BTreeRootPage(buffer),
						Type blpage when blpage == typeof(BTreeLeafPage) => new BTreeLeafPage(buffer),
						Type opopage when opopage == typeof(ObjectPageOverflow) => new ObjectPageOverflow(buffer),
						Type blopage when blopage == typeof(BTreeLeafPageOverflow) => new BTreeLeafPageOverflow(buffer),
						_ => throw new BarbadosException(BarbadosExceptionCode.InternalError),
					};
				}

				// If all pages in the cache are pinned, the page will be flushed to disk on the next save
				if (_cache.TryCache(handle, page))
				{
					_cache.Pin(handle);
				}

				return (T)page;
			}
		}

		public void Save(AbstractPage page)
		{
			if (!_cache.TryCache(page.Header.Handle, page))
			{
				_writePageBuffer(page);
				_cache.MarkClean(page.Header.Handle);
			}

			else
			{
				_cache.MarkDirty(page.Header.Handle);
			}
		}

		public void Release(AbstractPage page)
		{
			if (page is SlottedPage sp && sp.InternalModified)
			{
				sp.InternalModified = false;
				Save(sp);
			}

			_cache.Release(page.Header.Handle);
		}

		public void SaveRelease(AbstractPage page)
		{
			Save(page);
			Release(page);
		}

		private void _writePageBuffer(AbstractPage page)
		{
			DEBUG_ThrowUnallocatedHandle(page.Header.Handle);
			_writePageBuffer(_fileHandle, page);
		}

		private PageBuffer _readPageBuffer(PageHandle handle)
		{
			DEBUG_ThrowUnallocatedHandle(handle);
			return _readPageBuffer(_fileHandle, handle);
		}
	}
}
