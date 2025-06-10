namespace Barbados.StorageEngine.BTree.Pages
{
	internal partial class BTreeLeafPage
	{
		// Sync changes with 'Flags' when adding new types
		private enum EntryType : byte
		{
			Data,
			Overflow
		}
	}
}
