namespace Barbados.StorageEngine.Storage.Paging
{
	internal readonly struct PageHeader
	{
		public const int BinaryLength = PageHandle.BinaryLength + sizeof(byte);

		public PageHandle Handle { get; }
		public PageMarker Marker { get; }

		public PageHeader(PageHandle handle, PageMarker marker)
		{
			Handle = handle;
			Marker = marker;
		}
	}
}
