using System;
using System.Diagnostics;

using Barbados.StorageEngine.Storage;
using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.BTree.Pages
{
	internal sealed partial class BTreePage : SlottedPage
	{
		public new const int ThisHeaderLength = 0;
		//public new const int HeaderLength = SlottedPage.HeaderLength +
		//public new const int PayloadLength = SlottedPage.PayloadLength +
		public new const int WorstCaseFixedLengthOverheadPerEntry =
			SlottedPage.WorstCaseFixedLengthOverheadPerEntry + PageHandle.BinaryLength;

		public int Count => ActiveDescriptors.Count;
		public bool IsUnderflowed => SlottedHeader.UnoccupiedPercentage > 0.5;

		/* 
		 * payload:
		 *	separator1, PageHandle1; separator2, PageHandle2, ...
		 *	
		 *	'PageHandle' of the subtree where every separator is less or equal to the current separator
		 */

		public BTreePage(PageHandle handle) : base(ThisHeaderLength, new PageHeader(handle, PageMarker.BTreeNode))
		{

		}

		public BTreePage(PageBuffer buffer) : base(buffer)
		{
			ReadBaseAndGetStartBufferOffset();
			Debug.Assert(Header.Marker == PageMarker.BTreeNode);
		}

		public bool CanFit(int length)
		{
			return CanAllocate(length, PageHandle.BinaryLength);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public bool TryReadLowestSeparatorHandle(out BTreeNormalisedValueSpan separator, out PageHandle lessOrEqual)
		{
			if (ActiveDescriptors.TryGetLowest(out var descriptor))
			{
				var slot = GetSlot(descriptor);
				separator = BTreeNormalisedValueSpan.FromNormalised(slot.Key);
				lessOrEqual = HelpRead.AsPageHandle(slot.Data);
				return true;
			}

			separator = default;
			lessOrEqual = default!;
			return false;
		}

		public bool TryReadHighestSeparatorHandle(out BTreeNormalisedValueSpan separator, out PageHandle lessOrEqual)
		{
			if (ActiveDescriptors.TryGetHighest(out var descriptor))
			{
				var slot = GetSlot(descriptor);
				separator = BTreeNormalisedValueSpan.FromNormalised(slot.Key);
				lessOrEqual = HelpRead.AsPageHandle(slot.Data);
				return true;
			}

			separator = default;
			lessOrEqual = default!;
			return false;
		}

		public bool TryReadSubtreeHandle(BTreeNormalisedValueSpan key, out PageHandle subtree)
		{
			int index = ActiveDescriptors.BinarySearch(key.Bytes);
			if (index < 0)
			{
				index = ~index;

				// Current key would be inserted after the last existing descriptor, which means no matching subtree exists
				if (index >= ActiveDescriptors.Count)
				{
					subtree = default!;
					return false;
				}
			}

			// We either had an exact match or an index is currently at the next subtree handle
			var descriptor = ActiveDescriptors.Get(index);
			var slot = GetSlot(descriptor);
			subtree = HelpRead.AsPageHandle(slot.Data);
			return true;
		}

		public bool TryReadSeparatorHandle(BTreeNormalisedValueSpan separator, out PageHandle lessOrEqual)
		{
			if (TryRead(separator.Bytes, out var data, out _))
			{
				lessOrEqual = HelpRead.AsPageHandle(data);
				return true;
			}

			lessOrEqual = default!;
			return false;
		}

		public bool TryWriteSeparatorHandle(BTreeNormalisedValueSpan separator, PageHandle lessOrEqual)
		{
			Span<byte> h = stackalloc byte[PageHandle.BinaryLength];
			HelpWrite.AsPageHandle(h, lessOrEqual);
			return TryWrite(separator.Bytes, h);
		}

		public bool TryUpdateSeparatorHandle(BTreeNormalisedValueSpan separator, PageHandle lessOrEqual)
		{
			if (TryRead(separator.Bytes, out var data, out _))
			{
				HelpWrite.AsPageHandle(data, lessOrEqual);
				return true;
			}

			return false;
		}

		public bool TryRemoveSeparatorHandle(BTreeNormalisedValueSpan separator)
		{
			return TryRemove(separator.Bytes);
		}

		public void Spill(BTreePage to, bool fromHighest)
		{
			_spill(to, flush: false, fromHighest);
			Debug.Assert(Count != 0);
			Debug.Assert(to.Count != 0);
		}

		public void Flush(BTreePage to, bool fromHighest)
		{
			_spill(to, flush: true, fromHighest);
			Debug.Assert(Count == 0);
			Debug.Assert(to.Count != 0);
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			WriteBaseAndGetStartBufferOffset();
			return PageBuffer;
		}

		private void _spill(BTreePage to, bool flush, bool fromHighest)
		{
			var count = Count;
			while (
				(flush || (to.IsUnderflowed && !IsUnderflowed && count > 1)) &&
				(fromHighest ? TryReadHighestSeparatorHandle(out var sep, out var h) : TryReadLowestSeparatorHandle(out sep, out h))
			)
			{
				if (to.TryWriteSeparatorHandle(sep, h))
				{
					var r = TryRemoveSeparatorHandle(sep);
					Debug.Assert(r);
				}

				else
				{
					break;
				}

				count -= 1;
			}
		}
	}
}
