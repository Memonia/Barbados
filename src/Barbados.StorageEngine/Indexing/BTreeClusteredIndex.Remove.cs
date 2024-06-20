using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeClusteredIndex
	{
		public bool TryRemove(ObjectIdNormalised id)
		{
			var ikey = _toBTreeIndexKey(id);
			if (!TryFind(ikey, out var traceback))
			{
				return false;
			}

			var leaf = Pool.LoadPin<ObjectPage>(traceback.Current);

			// An object that didn't fit on the leaf page is spread across several overflow pages.
			// Since a single overflow chain is occupied by a single object, we can just
			// deallocate all overflow pages and remove the head entry from the leaf
			if (leaf.TryRemoveObjectChunk(id, out var next))
			{
				while (!next.IsNull)
				{
					var opage = Pool.LoadPin<ObjectPageOverflow>(next);
					next = opage.Next;

					Pool.Release(opage);
					Pool.Deallocate(opage.Header.Handle);
				}
			}

			else
			if (!leaf.TryRemoveObject(id))
			{
				Pool.Release(leaf);
				return false;
			}

			HandlePostRemoval(leaf, ikey.Separator, traceback);
			return true;
		}
	}
}
