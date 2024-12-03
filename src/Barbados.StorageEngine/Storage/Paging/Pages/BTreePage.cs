using System;
using System.Diagnostics;

using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal partial class BTreePage : SlottedPage
	{
		private const int _headerLength = 0;
		private const int _payloadFixedLengthPart = Constants.PageHandleLength;

		static BTreePage()
		{
			DebugHelpers.AssertBTreePageMinKeyCount(_headerLength, _payloadFixedLengthPart);
		}

		public bool IsUnderflowed => SlottedHeader.UnoccupiedPercentage > 0.5;

		/* 
		 * payload:
		 *	separator1, PageHandle1; separator2, PageHandle2, ...
		 *	
		 *	'PageHandle' of the subtree where every separator is less or equal to the current separator
		 */

		public BTreePage(PageHandle handle) : base(_headerLength, new PageHeader(handle, PageMarker.BTreeNode))
		{

		}

		public BTreePage(PageBuffer buffer) : base(buffer)
		{
			if (GetPageMarker(buffer) == PageMarker.BTreeNode)
			{
				ReadBaseAndGetStartBufferOffset();
				Debug.Assert(Header.Marker == PageMarker.BTreeNode);
			}

			else
			{
				Debug.Assert(
					GetPageMarker(buffer) == PageMarker.BTreeRoot ||
					GetPageMarker(buffer) == PageMarker.Collection
				);
			}
		}

		protected BTreePage(ushort headerLength, PageHeader pageHeader) : 
			base((ushort)(headerLength + _headerLength), pageHeader)
		{
			Debug.Assert(_headerLength + headerLength <= ushort.MaxValue);
			DebugHelpers.AssertBTreePageMinKeyCount(
				(ushort)(headerLength + _headerLength), _payloadFixedLengthPart
			);
		}

		public bool CanFit(int length)
		{
			return CanAllocate(length, Constants.PageHandleLength);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public bool TryReadLowestSeparatorHandle(out NormalisedValueSpan separator, out PageHandle lessOrEqual)
		{
			if (TryReadFromLowest(out var key, out var data, out _))
			{
				separator = NormalisedValueSpan.FromNormalised(key);
				lessOrEqual = HelpRead.AsPageHandle(data);
				return true;
			}

			separator = default;
			lessOrEqual = default!;
			return false;
		}

		public bool TryReadHighestSeparatorHandle(out NormalisedValueSpan separator, out PageHandle lessOrEqual)
		{
			if (TryReadFromHighest(out var key, out var data, out _))
			{
				separator = NormalisedValueSpan.FromNormalised(key);
				lessOrEqual = HelpRead.AsPageHandle(data);
				return true;
			}

			separator = default;
			lessOrEqual = default!;
			return false;
		}

		public bool TryReadSubtreeHandle(NormalisedValueSpan key, out PageHandle subtree)
		{
			/* The result of the binary search on the given key is one of the following:
			 * 
			 * 	1. The index of one of the descriptors with a given key, amongs which there's one
			 * 	active descriptor and the rest are garbage
			 * 
			 * 	2. The two's complement of an index of the first descriptor, whose key goes immediately
			 * 	after a given key 
			 */

			int index = DescriptorBinarySearch(key.Bytes);
			if (index < 0)
			{
				index = ~index;
			}

			else
			{
				// This is the first case. We know that binary search ended up with a match.
				// First we need to check whether there's an active descriptor to the left, because
				// the search might have jumped over it.
				for (int i = index - 1; i >= 0; --i)
				{
					var slot = GetSlot(Descriptors[i]);
					if (!slot.Key.SequenceEqual(key.Bytes))
					{
						break;
					}

					if (!Descriptors[i].IsGarbage)
					{
						subtree = HelpRead.AsPageHandle(slot.Data);
						return true;
					}
				}
			}

			// This is the second case. There's either no matching key or no active descriptors to the left.
			// Moving to the right, we will either find the exact match or the first active descriptor whose key
			// is less or equal to the given key, which is the one we're looking for.
			for (int i = index; i < Descriptors.Count; ++i)
			{
				var slot = GetSlot(Descriptors[i]);
				if (!Descriptors[i].IsGarbage && slot.Key.SequenceCompareTo(key.Bytes) >= 0)
				{
					subtree = HelpRead.AsPageHandle(slot.Data);
					return true;
				}
			}

			subtree = default;
			return false;
		}

		public bool TryReadSeparatorHandle(NormalisedValueSpan separator, out PageHandle lessOrEqual)
		{
			if (TryRead(separator.Bytes, out var data, out _))
			{
				lessOrEqual = HelpRead.AsPageHandle(data);
				return true;
			}

			lessOrEqual = default!;
			return false;
		}

		public bool TryWriteSeparatorHandle(NormalisedValueSpan separator, PageHandle lessOrEqual)
		{
			Span<byte> h = stackalloc byte[Constants.PageHandleLength];
			HelpWrite.AsPageHandle(h, lessOrEqual);
			return TryWrite(separator.Bytes, h);
		}

		public bool TryUpdateSeparatorHandle(NormalisedValueSpan separator, PageHandle lessOrEqual)
		{
			if (TryRead(separator.Bytes, out var data, out _))
			{
				HelpWrite.AsPageHandle(data, lessOrEqual);
				return true;
			}

			return false;
		}

		public bool TryRemoveSeparatorHandle(NormalisedValueSpan separator)
		{
			return TryRemove(separator.Bytes);
		}

		public void Spill(BTreePage to, bool fromHighest)
		{
			Spill(to, flush: false, fromHighest);
			Debug.Assert(Count() != 0);
			Debug.Assert(to.Count() != 0);
		}

		public void Flush(BTreePage to, bool fromHighest)
		{
			Spill(to, flush: true, fromHighest);
			Debug.Assert(Count() == 0);
			Debug.Assert(to.Count() != 0);
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			WriteBaseAndGetStartBufferOffset();
			return PageBuffer;
		}

		protected void Spill(BTreePage to, bool flush, bool fromHighest)
		{
			var count = Count();
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
