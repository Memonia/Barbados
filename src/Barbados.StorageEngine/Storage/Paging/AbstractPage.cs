namespace Barbados.StorageEngine.Storage.Paging
{
	internal abstract class AbstractPage
	{
		// sizeof(checksum)
		public const int ThisHeaderLength = sizeof(uint) + PageHeader.BinaryLength;
		public const int HeaderLength = ThisHeaderLength;
		public const int PayloadLength = Constants.PageLength - ThisHeaderLength;

		public static void WriteChecksum(PageBuffer buffer)
		{
			var span = buffer.AsSpan();
			var checksum = Crc32.Calculate(span[sizeof(uint)..]);
			HelpWrite.AsUInt32(span, checksum);
		}

		public static bool VerifyChecksum(PageBuffer buffer)
		{
			var span = buffer.AsSpan();
			var checksum = Crc32.Calculate(span[sizeof(uint)..]);
			return checksum == HelpRead.AsUInt32(span);
		}

		public static PageHandle GetPageHandle(PageBuffer buffer)
		{
			var span = buffer.AsSpan();
			return HelpRead.AsPageHandle(span[sizeof(uint)..]);

		}

		public static PageMarker GetPageMarker(PageBuffer buffer)
		{
			var span = buffer.AsSpan();
			return HelpRead.AsPageMarker(span[(sizeof(uint) + PageHandle.BinaryLength)..]);
		}

		// layout: U32 Checksum, header, derived page data 

		public PageHeader Header { get; private set; }

		protected PageBuffer PageBuffer { get; }

		protected AbstractPage(PageHeader header, PageBuffer buffer)
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
			var i = sizeof(uint);
			var span = PageBuffer.AsSpan();

			Header = new(
				HelpRead.AsPageHandle(span[i..]),
				HelpRead.AsPageMarker(span[(i + PageHandle.BinaryLength)..])
			);

			i += PageHeader.BinaryLength;
			return i;
		}

		virtual protected int WriteBaseAndGetStartBufferOffset()
		{
			var i = sizeof(uint);
			var span = PageBuffer.AsSpan();

			HelpWrite.AsPageHandle(span[i..], Header.Handle);
			i += PageHandle.BinaryLength;
			HelpWrite.AsPageMarker(span[i..], Header.Marker);
			i += sizeof(PageMarker);

			return i;
		}

		public abstract PageBuffer UpdateAndGetBuffer();
	}
}
