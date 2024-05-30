using System.Diagnostics;
using System.IO;

using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Paging
{
	internal partial class PagePool
	{
		[Conditional("DEBUG")]
		private void DEBUG_ThrowUnallocatedHandle(PageHandle handle)
		{
			/* Duplicating code here so that there are no infinite loops caused by the debug code 
			 */

			if (!_cache.TryGet(PageHandle.Root, out var page))
			{
				var buffer = new PageBuffer();
				RandomAccess.Read(_fileHandle, buffer.AsSpan(), PageHandle.Root.GetAddress());
				page = new RootPage(buffer);
			}

			var root = (RootPage)page;
			if (handle.Handle >= root.NextAvailablePageHandle.Handle)
			{
				Debug.Fail($"Unallocated handle: {handle}");
			}
		}
	}
}
