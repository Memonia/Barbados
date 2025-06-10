using Barbados.StorageEngine.BTree.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.BTree
{
	internal partial class BTreeContext
	{
		public static BTreeInfo CreateBTree(TransactionScope scope)
		{
			var h = scope.AllocateHandle();
			var root = new BTreePage(h);
			scope.Save(root);

			return new BTreeInfo(h);
		}
	}
}
