namespace Barbados.StorageEngine.Storage.Paging
{
	internal readonly struct PageHeader(PageHandle handle, PageMarker marker)
	{
		public PageHandle Handle { get; } = handle;
		public PageMarker Marker { get; } = marker;
	}
}
