namespace Barbados.StorageEngine.Paging.Metadata
{
	public enum PageMarker : byte
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
