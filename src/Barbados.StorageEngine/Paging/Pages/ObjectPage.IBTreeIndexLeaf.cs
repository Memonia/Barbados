using System.Diagnostics;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal partial class ObjectPage
	{
		public bool IsUnderflowed => SlottedHeader.UnoccupiedPercentage > 0.5;

		bool IBTreeIndexLeaf<ObjectPage>.TryReadLowest(out BTreeIndexKey key)
		{
			if (TryReadFromLowest(out var lkey, out _, out _))
			{
				key = new(NormalisedValueSpan.FromNormalised(lkey), false);
				return true;
			}

			key = default!;
			return false;
		}

		bool IBTreeIndexLeaf<ObjectPage>.TryReadHighest(out BTreeIndexKey key)
		{
			if (TryReadFromHighest(out var hkey, out _, out _))
			{
				key = new(NormalisedValueSpan.FromNormalised(hkey), false);
				return true;
			}

			key = default!;
			return false;
		}

		public void Spill(ObjectPage to, bool fromHighest)
		{
			_spill(to, flush: false, fromHighest);
		}

		public void Flush(ObjectPage to, bool fromHighest)
		{
			_spill(to, flush: true, fromHighest);
		}

		private void _spill(ObjectPage to, bool flush, bool fromHighest)
		{
			var count = Count();
			while (
				(flush || (to.IsUnderflowed && !IsUnderflowed && count > 1)) &&
				(fromHighest ? TryReadHighestId(out var id) : TryReadLowestId(out id))
			)
			{         
				var idn = new ObjectIdNormalised(id);
				if (TryReadObject(idn, out var obj))
				{
					if (to.TryWriteObject(idn, obj))
					{
						var r = TryRemoveObject(idn);
						Debug.Assert(r);
					}

					else
					{
						break;
					}
				}

				else
				if (TryReadObjectChunk(idn, out var chunk, out var length, out var handle))
				{
					if (to.TryWriteObjectChunk(idn, chunk, length, handle))
					{
						var r = TryRemoveObjectChunk(idn, out var storedHandle);
						Debug.Assert(r);
						Debug.Assert(storedHandle.Handle == handle.Handle);
					}

					else
					{
						break;
					}
				}

				else
				{
					Debug.Assert(false);
				}

				count -= 1;
			}
		}
	}
}
