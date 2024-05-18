using System.Diagnostics;

using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal sealed class BTreeRootPage : BTreePage
	{
		private const ushort _headerLength = 0;

		public BTreeRootPage(PageHandle handle) : base(_headerLength, new PageHeader(handle, PageMarker.BTreeRoot))
		{

		}

		public BTreeRootPage(PageBuffer buffer) : base(buffer)
		{
			var i = ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Debug.Assert(Header.Marker == PageMarker.BTreeRoot);
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			var i = WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			return PageBuffer;
		}
	}
}
