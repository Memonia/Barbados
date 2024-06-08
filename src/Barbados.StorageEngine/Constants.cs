using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine
{
	internal static class Constants
	{
		public const uint Version = 1;
		public const ulong MagicNumber = 0x534F444142524142;

		public const int PageLength = 4096;
		public const int PageHandleLength = sizeof(long);
		public const int PageHeaderLength = PageHandleLength + sizeof(PageMarker);

		public const int AllocationBitmapOverheadLength = PageHeaderLength;
		public const int AllocationBitmapLength = PageLength - AllocationBitmapOverheadLength;
		public const int AllocationBitmapPageCount = AllocationBitmapLength * 8;

		public const int SlottedPagePayloadLength = PageLength - SlottedPageOverheadLength;
		public const int SlottedPageOverheadLength = PageHeaderLength + SlottedPage.SlottedPageHeader.BinaryLength;

		public const int ObjectIdLength = sizeof(ulong);
		public const int ObjectIdNormalisedLength = ObjectIdLength;

		// See 'DebugHelpers'
		public const int IndexKeyMaxLength = 1000;
		// See 'DebugHelpers'
		public const int ObjectPageMaxChunkLength = 1000;
		
		public const int MinimalMaxIndexKeyLength = 1;
		public const int DefaultMaxIndexKeyLength = 256;
	}
}
