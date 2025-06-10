using System;
using System.Diagnostics;

using Barbados.StorageEngine.Storage;
using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.BTree.Pages
{
	internal sealed partial class BTreeLeafPage : SlottedPage, ITwoWayChainPage
	{
		public new const int ThisHeaderLength = PageHandle.BinaryLength * 2;
		public new const int HeaderLength = SlottedPage.HeaderLength + ThisHeaderLength;
		public new const int PayloadLength = SlottedPage.PayloadLength - ThisHeaderLength;
		public new const int WorstCaseFixedLengthOverheadPerEntry =
			SlottedPage.WorstCaseFixedLengthOverheadPerEntry + OverflowInfo.BinaryLength;

		public int Count => ActiveDescriptors.Count;
		public bool IsUnderflowed => SlottedHeader.UnoccupiedPercentage > 0.5;

		/* Try(Write/Read/Delete)Data methods only deal with inline entries. If either the key is trimmed or 
		 * the data cannot fit on a page, it must be handled separately and a record of it is manipulated via 
		 * Try(Write/Read/Delete)OverflowInfo
		 */

		public PageHandle Next { get; set; }
		public PageHandle Previous { get; set; }

		public BTreeLeafPage(PageHandle handle) : base(ThisHeaderLength, new PageHeader(handle, PageMarker.BTreeLeaf))
		{
			Next = PageHandle.Null;
			Previous = PageHandle.Null;
		}

		public BTreeLeafPage(PageBuffer buffer) : base(buffer)
		{
			var i = base.ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Next = HelpRead.AsPageHandle(span[i..]);
			i += PageHandle.BinaryLength;
			Previous = HelpRead.AsPageHandle(span[i..]);
			i += PageHandle.BinaryLength;

			Debug.Assert(Header.Marker == PageMarker.BTreeLeaf);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public void Spill(BTreeLeafPage to, bool fromHighest)
		{
			_spill(to, flush: false, fromHighest);
		}

		public void Flush(BTreeLeafPage to, bool fromHighest)
		{
			_spill(to, flush: true, fromHighest);
		}

		public bool Exists(BTreeLookupKeySpan key)
		{
			return TryRead(key.Separator.Bytes, out _, out _);
		}

		public bool TryGetKey(int index, out BTreeLookupKeySpan key)
		{
			if (index < 0 || index >= ActiveDescriptors.Count)
			{
				key = default!;
				return false;
			}

			var slot = GetSlot(ActiveDescriptors.Get(index));
			key = new(BTreeNormalisedValueSpan.FromNormalised(slot.Key), ((Flags)slot.Flags).IsKeyTrimmed);
			return true;
		}

		public bool TryGetLowestKey(out BTreeLookupKeySpan key)
		{
			if (ActiveDescriptors.TryGetLowest(out var descriptor))
			{
				var slot = GetSlot(descriptor);
				key = new(BTreeNormalisedValueSpan.FromNormalised(slot.Key), ((Flags)slot.Flags).IsKeyTrimmed);
				return true;
			}

			key = default!;
			return false;
		}

		public bool TryGetHighestKey(out BTreeLookupKeySpan key)
		{
			if (ActiveDescriptors.TryGetHighest(out var descriptor))
			{
				var slot = GetSlot(descriptor);
				key = new(BTreeNormalisedValueSpan.FromNormalised(slot.Key), ((Flags)slot.Flags).IsKeyTrimmed);
				return true;
			}

			key = default!;
			return false;
		}

		public bool TryWriteData(BTreeLookupKeySpan key, ReadOnlySpan<byte> data)
		{
			Debug.Assert(!key.IsTrimmed);
			if (!TryWrite(key.Separator.Bytes, data))
			{
				return false;
			}

			var flags = new Flags(key.IsTrimmed, EntryType.Data);
			var r = TrySetFlags(key.Separator.Bytes, (byte)flags);
			Debug.Assert(r);
			return true;
		}

		public bool TryWriteOverflowInfo(BTreeLookupKeySpan key, OverflowInfo info)
		{
			if (!TryAllocate(key.Separator.Bytes, OverflowInfo.BinaryLength, out var span))
			{
				return false;
			}

			var flags = new Flags(key.IsTrimmed, EntryType.Overflow);
			var r = TrySetFlags(key.Separator.Bytes, (byte)flags);
			Debug.Assert(r);

			info.WriteTo(span);
			return true;
		}

		public bool TryReadData(BTreeLookupKeySpan key, out ReadOnlySpan<byte> data)
		{
			if (!_tryReadEntry(key, EntryType.Data, out var span, out var flags))
			{
				data = default!;
				return false;
			}

			Debug.Assert(!flags.IsKeyTrimmed);

			data = span;
			return true;
		}

		public bool TryReadOverflowInfo(BTreeLookupKeySpan key, out OverflowInfo info)
		{
			if (!_tryReadEntry(key, EntryType.Overflow, out var span, out _))
			{
				info = default!;
				return false;
			}

			info = OverflowInfo.ReadFrom(span);
			return true;
		}

		public bool TryRemoveData(BTreeLookupKeySpan key)
		{
			if (!TryRead(key.Separator.Bytes, out _, out var flags))
			{
				return false;
			}

			var f = (Flags)flags;
			if (f.EntryType != EntryType.Data || f.IsKeyTrimmed)
			{
				return false;
			}

			var r = TryRemove(key.Separator.Bytes);
			Debug.Assert(r);
			return true;
		}

		public bool TryRemoveOverflowInfo(BTreeLookupKeySpan key, out OverflowInfo info)
		{
			if (!_tryReadEntry(key, EntryType.Overflow, out var span, out _))
			{
				info = default!;
				return false;
			}

			info = OverflowInfo.ReadFrom(span);
			return TryRemove(key.Separator.Bytes);
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			var i = base.WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsPageHandle(span[i..], Next);
			i += PageHandle.BinaryLength;
			HelpWrite.AsPageHandle(span[i..], Previous);
			i += PageHandle.BinaryLength;

			return PageBuffer;
		}

		private bool _tryReadEntry(BTreeLookupKeySpan key, EntryType entryType, out Span<byte> data, out Flags flags)
		{
			if (!TryRead(key.Separator.Bytes, out data, out var f) || ((Flags)f).EntryType != entryType)
			{
				flags = default;
				return false;
			}

			flags = (Flags)f;
			return true;
		}

		private void _spill(BTreeLeafPage to, bool flush, bool fromHighest)
		{
			var count = Count;
			while (
				(flush || (to.IsUnderflowed && !IsUnderflowed && count > 1)) &&
				(fromHighest ? TryGetHighestKey(out var key) : TryGetLowestKey(out key))
			)
			{
				if (TryReadData(key, out var data))
				{
					if (to.TryWriteData(key, data))
					{
						var r = TryRemoveData(key);
						Debug.Assert(r);
					}

					else
					{
						break;
					}
				}

				else
				{
					var r = TryReadOverflowInfo(key, out var info);
					Debug.Assert(r);

					if (to.TryWriteOverflowInfo(key, info))
					{
						r = TryRemoveOverflowInfo(key, out _);
						Debug.Assert(r);
					}

					else
					{
						break;
					}
				}

				count -= 1;
			}
		}
	}
}
