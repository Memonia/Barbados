using System.Diagnostics;

using Barbados.StorageEngine.Storage.Paging;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Indexing
{
	internal abstract class AbstractBTreeIndexFacade
	{
		public BTreeIndexInfo Info { get; }

		protected AbstractBTreeIndexFacade(BTreeIndexInfo info)
		{
			Info = info;
		}

		public void Deallocate(TransactionScope transaction)
		{
			void _deallocate(PageHandle handle)
			{
				if (
					handle.Handle != Info.RootHandle.Handle &&
					!transaction.IsPageType(handle, PageMarker.BTreeNode)
				)
				{
					transaction.Deallocate(handle);
					return;
				}

				var node = transaction.Load<BTreePage>(handle);
				var e = node.GetEnumerator();
				while (e.TryGetNext(out var separator))
				{
					var r = node.TryReadSeparatorHandle(separator, out var lessOrEqual);
					Debug.Assert(r);

					_deallocate(lessOrEqual);
				}

				transaction.Deallocate(handle);
			}

			_deallocate(Info.RootHandle);
		}

		public BTreeIndexKey ToBTreeIndexKey(NormalisedValue key)
		{
			var kb = key.AsSpan().Bytes;
			return kb.Length > Info.KeyMaxLength
				? new(NormalisedValueSpan.FromNormalised(kb[..Info.KeyMaxLength]), true)
				: new(new(key), false);
		}

		public BTreeIndexKey ToBTreeIndexKey<T>(T key)
		{
			return ToBTreeIndexKey(NormalisedValue.Create(key));
		}
	}
}
