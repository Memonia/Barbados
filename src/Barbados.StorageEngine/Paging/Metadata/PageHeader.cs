namespace Barbados.StorageEngine.Paging.Metadata
{
	internal readonly struct PageHeader(PageHandle handle, PageMarker marker)
	{
		public PageHandle Handle { get; } = handle;
		public PageMarker Marker { get; } = marker;
	}
}
