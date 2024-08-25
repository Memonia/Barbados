using System.Diagnostics;

using Barbados.StorageEngine.Helpers;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal sealed class CollectionPage : BTreePage
	{
		private const ushort _headerLength = Constants.ObjectIdLength;

		public ObjectId NextObjectId { get; private set; }

		public CollectionPage(PageHandle handle) : base(_headerLength, new PageHeader(handle, PageMarker.Collection))
		{
			// Monotonically decreasing keys are faster to insert into a clustered index.
			// Every time the greatest key in a page changes, this needs to be propagated to the parent pages.
			// Inserting keys in increasing order makes the update propagate on every single insert. On the other hand,
			// inserting keys in decreasing order makes the update propagate only on the first insert into the page or on a split 
			NextObjectId = ObjectId.MaxValue;
		}

		public CollectionPage(PageBuffer buffer) : base(buffer)
		{
			var i = ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			NextObjectId = HelpRead.AsObjectId(span[i..]);
			i += Constants.ObjectIdLength;

			Debug.Assert(Header.Marker == PageMarker.Collection);
		}

		public bool TryGetNextObjectId(out ObjectId id)
		{
			if (NextObjectId.Value == ObjectId.Invalid.Value)
			{
				id = default!;
				return false;
			}

			id = NextObjectId;
			NextObjectId = new(id.Value - 1);
			return true;
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			var i = WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsObjectId(span[i..], NextObjectId);
			i += Constants.ObjectIdLength;

			return PageBuffer;
		}
	}
}
