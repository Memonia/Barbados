using System;
using System.Diagnostics;

using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal sealed partial class ObjectPageOverflow : SlottedPage, IOneWayChainPage
	{
		private const int _headerLength = Constants.PageHandleLength;

		public PageHandle Next { get; set; }

		/* payload:
		 *	ObjectId1, chunk1; ObjectId2, chunk2, ...
		 */

		public ObjectPageOverflow(PageHandle handle) :
			base(_headerLength, new PageHeader(handle, PageMarker.ObjectOverflow))
		{
			Next = PageHandle.Null;
		}

		public ObjectPageOverflow(PageBuffer buffer) : base(buffer)
		{
			var i = ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Next = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;

			Debug.Assert(Header.Marker == PageMarker.ObjectOverflow);
		}

		public bool TryReadObjectChunk(ObjectIdNormalised id, out Span<byte> chunk)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			return TryRead(key, out chunk, out _);
		}

		public bool TryWriteObjectChunk(ObjectIdNormalised id, ReadOnlySpan<byte> obj, out int written)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			var free = GetMaxAllocatableRegionLength();
			if (free > Constants.ObjectIdNormalisedLength + 1)
			{
				written = obj.Length > free - Constants.ObjectIdNormalisedLength
					? free - Constants.ObjectIdNormalisedLength 
					: obj.Length;

				var r = TryAllocate(key, written, out var span);
				Debug.Assert(r);

				obj[..written].CopyTo(span);
				return true;
			}

			written = default!;
			return false;
		}

		public bool TryRemoveObjectChunk(ObjectIdNormalised id)
		{
			Span<byte> key = stackalloc byte[Constants.ObjectIdNormalisedLength];
			id.WriteTo(key);

			return TryRemove(key);
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
