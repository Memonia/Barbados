using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeClusteredIndexTransactionProxy
	{
		public bool TryRemove(ObjectIdNormalised id)
		{
			var ikey = _toBTreeIndexKey(id);
			if (!TryFind(ikey, out var traceback))
			{
				return false;
			}

			var leaf = Transaction.Load<ObjectPage>(traceback.Current);

			// An object that didn't fit on the leaf page is spread across several overflow pages.
			// Since a single overflow chain is occupied by a single object, we can just
			// deallocate all overflow pages and remove the head entry from the leaf
			if (leaf.TryRemoveObjectChunk(id, out var next))
			{
				while (!next.IsNull)
				{
					var opage = Transaction.Load<ObjectPageOverflow>(next);
					next = opage.Next;
					Transaction.Deallocate(opage.Header.Handle);
				}
			}

			else
			if (!leaf.TryRemoveObject(id))
			{
				return false;
			}

			HandlePostRemoval(leaf, ikey.Separator, traceback);
			return true;
		}
	}
}
