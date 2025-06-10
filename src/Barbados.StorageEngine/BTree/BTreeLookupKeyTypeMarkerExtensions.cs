namespace Barbados.StorageEngine.BTree
{
	internal static class BTreeLookupKeyTypeMarkerExtensions
	{
		public static bool IsInternalMarker(this BTreeLookupKeyTypeMarker marker)
		{
			return marker < BTreeLookupKeyTypeMarker.External;
		}
	}
}
