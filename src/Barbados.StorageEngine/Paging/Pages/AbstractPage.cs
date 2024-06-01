using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal abstract class AbstractPage
	{
		public static PageMarker GetPageMarker(PageBuffer buffer)
		{
			return HelpRead.AsPageMarker(buffer.AsSpan()[Constants.PageHandleLength..]);
		}

		public PageHeader Header { get; private set; }

		protected PageBuffer PageBuffer { get; }

		public AbstractPage(PageHeader header, PageBuffer buffer)
		{
			Header = header;
			PageBuffer = buffer;
		}

		protected AbstractPage(PageHeader header) : this(header, new())
		{

		}

		protected AbstractPage(PageBuffer buffer) : this(new(), buffer)
		{

		}

		virtual protected int ReadBaseAndGetStartBufferOffset()
		{
			var span = PageBuffer.AsSpan();
			Header = new(
				HelpRead.AsPageHandle(span),
				HelpRead.AsPageMarker(span[Constants.PageHandleLength..])
			);

			return Constants.PageHandleLength + sizeof(PageMarker);
		}

		virtual protected int WriteBaseAndGetStartBufferOffset()
		{
			var i = 0;
			var span = PageBuffer.AsSpan();

			HelpWrite.AsPageHandle(span[i..], Header.Handle);
			i += Constants.PageHandleLength;
			HelpWrite.AsPageMarker(span[i..], Header.Marker);
			i += sizeof(PageMarker);

			return i;
		}

		public abstract PageBuffer UpdateAndGetBuffer();
	}
}
