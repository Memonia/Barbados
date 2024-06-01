using System.Diagnostics;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal sealed class RootPage : AbstractPage
	{
		public static void ThrowDatabaseDoesNotExist(PageBuffer buffer)
		{
			var magic = HelpRead.AsUInt64(buffer.AsSpan()[Constants.PageHeaderLength..]);
			if (magic != Constants.MagicNumber)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DatabaseDoesNotExist, "Could not validate the database file"
				);
			}
		}

		public static void ThrowDatabaseVersionMismatch(PageBuffer buffer)
		{
			var version = HelpRead.AsUInt32(buffer.AsSpan()[(Constants.PageHeaderLength + sizeof(ulong))..]);
			if (version != Constants.Version)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DatabaseVersionMismatch, 
					$"Unexpected database file version. Expected version {Constants.Version}, found version {version}"
				);
			}
		}

		public ulong Magic { get; }
		public uint Version { get; }

		public PageHandle NextAvailablePageHandle { get; private set; }

		public PageHandle MetaCollectionPageHandle { get; set; }
		public PageHandle MetaCollectionNameIndexRootPageHandle { get; set; }
		public PageHandle MetaCollectionClusteredIndexRootPageHandle { get; set; }
		public PageHandle AllocationPageChainHeadHandle { get; set; }

		public RootPage() : base(new PageHeader(PageHandle.Root, PageMarker.Root))
		{
			Version = Constants.Version;
			Magic = Constants.MagicNumber;

			NextAvailablePageHandle = new(PageHandle.Root.Handle + 1);

			AllocationPageChainHeadHandle = PageHandle.Null;
			MetaCollectionPageHandle = PageHandle.Null;
			MetaCollectionClusteredIndexRootPageHandle = PageHandle.Null;
		}

		public RootPage(PageBuffer buffer) : base(buffer)
		{
			var i = ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			Magic = HelpRead.AsUInt64(span[i..]);
			i += sizeof(ulong);
			Version = HelpRead.AsUInt32(span[i..]);
			i += sizeof(uint);
			NextAvailablePageHandle = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;
			AllocationPageChainHeadHandle = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;
			MetaCollectionPageHandle = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;
			MetaCollectionNameIndexRootPageHandle = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;
			MetaCollectionClusteredIndexRootPageHandle = HelpRead.AsPageHandle(span[i..]);
			i += Constants.PageHandleLength;

			Debug.Assert(Header.Marker == PageMarker.Root);
		}

		public PageHandle IncrementNextAvailablePageHandle()
		{
			if (!NextAvailablePageHandle.IsWithinBounds)
			{
				throw new BarbadosException(BarbadosExceptionCode.MaxPageCountReached);
			}

			var next = NextAvailablePageHandle;
			NextAvailablePageHandle = new(NextAvailablePageHandle.Handle + 1);
			return next;
		}

		public override PageBuffer UpdateAndGetBuffer()
		{
			var i = WriteBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			HelpWrite.AsUInt64(span[i..], Magic);
			i += sizeof(ulong);
			HelpWrite.AsUInt32(span[i..], Version);
			i += sizeof(uint);
			HelpWrite.AsPageHandle(span[i..], NextAvailablePageHandle);
			i += Constants.PageHandleLength;
			HelpWrite.AsPageHandle(span[i..], AllocationPageChainHeadHandle);
			i += Constants.PageHandleLength;
			HelpWrite.AsPageHandle(span[i..], MetaCollectionPageHandle);
			i += Constants.PageHandleLength;
			HelpWrite.AsPageHandle(span[i..], MetaCollectionNameIndexRootPageHandle);
			i += Constants.PageHandleLength;
			HelpWrite.AsPageHandle(span[i..], MetaCollectionClusteredIndexRootPageHandle);
			i += Constants.PageHandleLength;

			return PageBuffer;
		}
	}
}
