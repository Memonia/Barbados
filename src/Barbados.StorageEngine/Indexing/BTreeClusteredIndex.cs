using System;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed partial class BTreeClusteredIndex : AbstractBTreeIndex<ObjectPage>
	{
		/* Clustered indexes are protected by its collections' respective locks
		 */

		private static BTreeIndexKey _toBTreeIndexKey(ReadOnlySpan<byte> objectIdNormalisedBuffer)
		{
			return new(NormalisedValueSpan.FromNormalised(objectIdNormalisedBuffer), false);
		}

		public BTreeClusteredIndex(PagePool pool, PageHandle handle)
			: base(
				pool,
				new()
				{
					IndexedField = BarbadosIdentifiers.Id,
					RootPageHandle = handle,
					KeyMaxLength = Constants.ObjectIdLength
				}
			)
		{

		}

		public void DeallocateNoLock()
		{
			Deallocate();
		}

		public bool TryRead(ObjectIdNormalised id, out PageHandle handle)
		{
			Span<byte> kBuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(kBuf);

			var ikey = _toBTreeIndexKey(kBuf);
			if (TryFind(ikey, out var traceback))
			{
				handle = traceback.Current;
				return true;
			}

			handle = default!;
			return false;
		}

		public bool TryGetLeftmostLeafHandle(out PageHandle handle)
		{
			var min = new ObjectIdNormalised(ObjectId.MinValue);
			Span<byte> kBuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
			min.WriteTo(kBuf);

			var k = _toBTreeIndexKey(kBuf);
			if (TryFind(k, out var traceback))
			{
				handle = traceback.Current;
				return true;
			}

			handle = default!;
			return false;
		}

		public bool TryGetRightmostLeafHandle(out PageHandle handle)
		{
			var max = new ObjectIdNormalised(ObjectId.MaxValue);
			Span<byte> kBuf = stackalloc byte[Constants.ObjectIdNormalisedLength];
			max.WriteTo(kBuf);

			var k = _toBTreeIndexKey(kBuf);
			if (TryFind(k, out var traceback))
			{
				handle = traceback.Current;
				return true;
			}

			handle = default!;
			return false;
		}
	}
}
