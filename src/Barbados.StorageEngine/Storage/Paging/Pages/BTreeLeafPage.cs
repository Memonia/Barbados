using System.Diagnostics;

using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal sealed partial class BTreeLeafPage : SlottedPage, IBTreeIndexLeaf<BTreeLeafPage>
	{
		private const int _headerLength = Constants.PageHandleLength * 2;
		private const int _payloadFixedLengthPart = Constants.PageHandleLength;

		static BTreeLeafPage()
		{
			DebugHelpers.AssertBTreePageMinKeyCount(_headerLength, _payloadFixedLengthPart);

			// 'TryOverwriteWithOverflow' depends on this
			Debug.Assert(Constants.ObjectIdLength == Constants.PageHandleLength);
		}

		public bool IsUnderflowed => SlottedHeader.UnoccupiedPercentage > 0.5;

		public PageHandle Next { get; set; }
		public PageHandle Previous { get; set; }

		/* flags:
		 *  IsTrimmed, HasDuplicate
		 *  
		 * payload:
		 *  key1, entry1; key2; entry2, ...
		 *
		 * entry:
		 *  HasDuplicate == false:
		 *   ObjectId - id of the object
		 *  
		 *  HasDuplicate == true:
		 *   PageHandle - handle to the overflow page
		 */

		public BTreeLeafPage(PageHandle handle) : base(_headerLength, new PageHeader(handle, PageMarker.BTreeLeaf))
		{
			Next = PageHandle.Null;
			Previous = PageHandle.Null;
		}

		public BTreeLeafPage(PageBuffer buffer) : base(buffer)
		{
			var i = ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Next = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;
			Previous = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;

			Debug.Assert(Header.Marker == PageMarker.BTreeLeaf);
		}

		public new int Count()
		{
			return base.Count();
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public bool TryReadLowest(out NormalisedValueSpan key)
		{
			if (TryReadFromLowest(out var lkey, out _, out _))
			{
				key = NormalisedValueSpan.FromNormalised(lkey);
				return true;
			}

			key = default;
			return false;
		}

		public bool TryReadHighest(out NormalisedValueSpan key)
		{
			if (TryReadFromHighest(out var hkey, out _, out _))
			{
				key = NormalisedValueSpan.FromNormalised(hkey);
				return true;
			}

			key = default;
			return false;
		}

		public bool TryReadObjectId(NormalisedValueSpan key, out ObjectId id, out bool isTrimmed)
		{
			if (TryRead(key.Bytes, out var data, out var flags))
			{
				var eflags = new Flags(flags);
				if (!eflags.HasDuplicate)
				{
					id = HelpRead.AsObjectId(data);
					isTrimmed = eflags.IsTrimmed;
					return true;
				}
			}

			id = default!;
			isTrimmed = default!;
			return false;
		}

		public bool TryWriteObjectId(BTreeIndexKey key, ObjectId id)
		{
			if (TryAllocate(key.Separator.Bytes, Constants.ObjectIdLength, out var span))
			{
				HelpWrite.AsObjectId(span, id);
				var eflags = new Flags() { IsTrimmed = key.IsTrimmed };
				var r = TrySetFlags(key.Separator.Bytes, eflags);
				Debug.Assert(r);
				return true;
			}

			return false;
		}

		public bool TryRemoveObjectId(BTreeIndexKey key)
		{
			if (TryRead(key.Separator.Bytes, out _, out var flags))
			{
				var eflags = new Flags(flags);
				if (!eflags.HasDuplicate && eflags.IsTrimmed == key.IsTrimmed)
				{
					return TryRemove(key.Separator.Bytes);
				}
			}

			return false;
		}

		public bool TryOverwriteWithOverflow(NormalisedValueSpan key, PageHandle overflowHandle)
		{
			if (TryRead(key.Bytes, out var data, out var flags))
			{
				var eflags = new Flags(flags);
				if (eflags.HasDuplicate)
				{
					return false;
				}

				eflags = eflags with { HasDuplicate = true, IsTrimmed = false };
				var r = TrySetFlags(key.Bytes, eflags);
				Debug.Assert(r); 

				HelpWrite.AsPageHandle(data, overflowHandle);
				return true;
			}

			return false;
		}

		public bool TryReadOverflowHandle(NormalisedValueSpan key, out PageHandle overflowHandle)
		{
			if (TryRead(key.Bytes, out var data, out var flags))
			{
				var eflags = new Flags(flags);
				if (eflags.HasDuplicate)
				{
					overflowHandle = HelpRead.AsPageHandle(data);
					return true;
				}
			}

			overflowHandle = default!;
			return false;
		}

		public bool TryRemoveOverflowHandle(NormalisedValueSpan key, out PageHandle overflowHandle)
		{
			if (TryRead(key.Bytes, out var data, out var flags))
			{
				var eflags = new Flags(flags);
				if (eflags.HasDuplicate)
				{
					overflowHandle = HelpRead.AsPageHandle(data);
					return TryRemove(key.Bytes);
				}
			}

			overflowHandle = default!;
			return false;
		}

		public void Spill(BTreeLeafPage to, bool fromHighest)
		{
			_spill(to, flush: false, fromHighest);
			Debug.Assert(Count() != 0);
			Debug.Assert(to.Count() != 0);
		}

		public void Flush(BTreeLeafPage to, bool fromHighest)
		{
			_spill(to, flush: true, fromHighest);
			Debug.Assert(Count() == 0);
			Debug.Assert(to.Count() != 0);
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			var i = WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsPageHandle(span[i..], Next);
			i += Constants.PageHandleLength;
			HelpWrite.AsPageHandle(span[i..], Previous);
			i += Constants.PageHandleLength;

			return PageBuffer;
		}

		private void _spill(BTreeLeafPage to, bool flush, bool fromHighest)
		{
			var count = Count();
			while (
				(flush || (to.IsUnderflowed && !IsUnderflowed && count > 1)) &&
				(fromHighest ? TryReadHighest(out var key) : TryReadLowest(out key))
			)
			{
				if (TryReadObjectId(key, out var id, out var isTrimmed))
				{
					if (to.TryWriteObjectId(new(key, isTrimmed), id))
					{
						var r = TryRemove(key.Bytes);
						Debug.Assert(r);
					}

					else
					{
						break;
					}
				}

				else
				if (TryReadOverflowHandle(key, out var handle))
				{
					var k = new BTreeIndexKey(key, false);
					if (to.TryWriteObjectId(k, ObjectId.Invalid))
					{
						var r = to.TryOverwriteWithOverflow(k.Separator, handle);
						Debug.Assert(r);

						r = TryRemove(key.Bytes);
						Debug.Assert(r);
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
