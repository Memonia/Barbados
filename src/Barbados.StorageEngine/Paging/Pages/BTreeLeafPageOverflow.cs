using System.Diagnostics;

using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal sealed partial class BTreeLeafPageOverflow : SlottedPage, IOneWayChainPage
	{
		private const int _headerLength = Constants.PageHandleLength;

		public PageHandle Next { get; set; }

		/* flags:
		 *  isTrimmed
		 *  
		 * payload:
		 *	ObjectId1, ObjectId2, ...
		 *	
		 *	'ObjectId's are supposed to be unique, so they are used as keys
		 */

		public BTreeLeafPageOverflow(PageHandle handle) :
			base(_headerLength, new PageHeader(handle, PageMarker.BTreeLeafOverflow))
		{
			Next = PageHandle.Null;
		}

		public BTreeLeafPageOverflow(PageBuffer buffer) : base(buffer)
		{
			var i = ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Next = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;

			Debug.Assert(Header.Marker == PageMarker.BTreeLeafOverflow);
		}

		public new int Count()
		{
			return base.Count();
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public bool TryWriteObjectId(ObjectIdNormalised id, bool isTrimmed)
		{
			if (TryWrite(id, []))
			{
				var eflags = new BTreeLeafPage.Flags { IsTrimmed = isTrimmed };
				var r = TrySetFlags(id, eflags);
				Debug.Assert(r);

				return true;
			}

			return false;
		}

		public bool TryRemoveObjectId(ObjectIdNormalised id)
		{
			return TryRemove(id);
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			var i = WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsPageHandle(span[i..], Next);
			i += Constants.PageHandleLength;

			return PageBuffer;
		}
	}
}
