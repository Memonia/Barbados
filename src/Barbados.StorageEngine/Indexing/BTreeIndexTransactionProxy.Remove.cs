using System.Diagnostics;

using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal partial class BTreeIndexTransactionProxy
	{
		public bool TryRemove(BTreeIndexKey key, ObjectId id)
		{
			if (!TryFind(key, out var traceback))
			{
				return false;
			}

			var leaf = Transaction.Load<BTreeLeafPage>(traceback.Current);
			var removeOverflow = false;

			// The entry is in the overflow page. Remove it and deallocate the page if it's empty
			if (leaf.TryReadOverflowHandle(key.Separator, out var next))
			{
				Debug.Assert(!next.IsNull);
				var prev = PageHandle.Null;
				while (!next.IsNull)
				{
					var opage = Transaction.Load<BTreeLeafPageOverflow>(next);
					if (opage.TryRemoveObjectId(new(id)))
					{
						// Deallocate empty overflow page
						if (opage.Count() == 0)
						{
							// The only page in the chain
							if (prev.IsNull && opage.Next.IsNull)
							{
								removeOverflow = true;
							}

							// There are more pages in the chain
							if (!prev.IsNull)
							{
								var popage = Transaction.Load<BTreeLeafPageOverflow>(prev);
								ChainHelpers.RemoveOneWay(opage, popage);
								Transaction.Save(popage);
							}

							Transaction.Deallocate(opage.Header.Handle);
						}

						else
						{
							Transaction.Save(opage);
						}

						break;
					}

					else
					{
						prev = next;
						next = opage.Next;
					}
				}

				Transaction.Save(leaf);
			}

			else
			if (!leaf.TryRemoveObjectId(key))
			{
				return false;
			}

			if (removeOverflow)
			{
				var r = leaf.TryRemoveOverflowHandle(key.Separator, out _);
				Debug.Assert(r);
			}

			HandlePostRemoval(leaf, key.Separator, traceback);
			return true;
		}
	}
}
