namespace Barbados.StorageEngine.Storage.Paging
{
	internal enum PageMarker : byte
	{
		Root = 1,
		Allocation,
		BTreeNode = 8,
		BTreeLeaf
	}
}
