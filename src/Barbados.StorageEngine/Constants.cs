using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine
{
	internal static class Constants
	{
		public const uint DbVersion = 1;
		public const uint WalVersion = 1;
		public const ulong DbMagicNumber = 0x534F444142524142;
		public const ulong WalMagicNumber = 0x4C41572D534F4442;

		public const int PageLength = 4096;
		public const int PageHandleLength = sizeof(long);
		public const int PageHeaderLength = sizeof(uint) + PageHandleLength + sizeof(PageMarker);
		public const int WalHeaderLength = sizeof(ulong) + sizeof(ulong) + sizeof(uint);
		public const int WalRecordLength = sizeof(uint) + ObjectIdLength + sizeof(byte);

		public const int AllocationBitmapOverheadLength = PageHeaderLength;
		public const int AllocationBitmapLength = PageLength - AllocationBitmapOverheadLength;
		public const int AllocationBitmapPageCount = AllocationBitmapLength * 8;

		public const int SlottedPagePayloadLength = PageLength - SlottedPageOverheadLength;
		public const int SlottedPageOverheadLength = PageHeaderLength + SlottedPage.SlottedPageHeader.BinaryLength;

		public const int ObjectIdLength = sizeof(ulong);
		public const int ObjectIdNormalisedLength = ObjectIdLength;

		// See 'DebugHelpers'
		public const int IndexKeyMaxLength = 896;
		// See 'DebugHelpers'
		public const int ObjectPageMaxChunkLength = 896;

		public const int MinIndexKeyMaxLength = 1;
		public const int DefaultMaxIndexKeyLength = 256;
	}
}
