namespace Barbados.StorageEngine.Storage.Paging
{
	internal enum PageMarker : byte
	{
		Root = 1,
		Allocation,
		BTreeRoot,
		BTreeNode,
		BTreeLeaf,
		BTreeLeafOverflow,
		Collection,
		Object,
		ObjectOverflow
	}
}
