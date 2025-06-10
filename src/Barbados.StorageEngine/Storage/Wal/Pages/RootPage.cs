using System;
using System.Diagnostics;

using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Storage.Wal.Pages
{
	internal sealed class RootPage : AbstractPage
	{
		public static void ThrowDatabaseDoesNotExist(PageBuffer buffer)
		{
			var span = buffer.AsSpan();
			var magic = HelpRead.AsUInt64(span[(HeaderLength + sizeof(ulong))..]);
			if (magic != Constants.DbMagicNumber)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DatabaseDoesNotExist, "Could not validate the database file"
				);
			}
		}

		public static void ThrowDatabaseVersionMismatch(PageBuffer buffer)
		{
			var span = buffer.AsSpan();
			var version = HelpRead.AsUInt32(span[(HeaderLength + sizeof(ulong) * 2)..]);
			if (version != Constants.DbVersion)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DatabaseVersionMismatch,
					$"Unexpected database file version. Expected version {Constants.DbVersion}, found version {version}"
				);
			}
		}

		public ulong FileMagic { get; }
		public ulong BarbadosMagic { get; }

		public uint Version { get; }

		public PageHandle NextAvailablePageHandle { get; private set; }
		public PageHandle MetaCollectionPageHandle { get; set; }
		public PageHandle MetaCollectionNameIndexRootPageHandle { get; set; }
		public PageHandle FirstAllocationPageHandle { get; set; }
		public PageHandle LastAllocationPageHandle { get; set; }

		public RootPage() : base(new PageHeader(PageHandle.Root, PageMarker.Root))
		{
			var r = new Random();
			FileMagic = (ulong)r.NextInt64() | 1UL << 63;
			BarbadosMagic = Constants.DbMagicNumber;
			Version = Constants.DbVersion;
			NextAvailablePageHandle = new(PageHandle.Root.Handle + 1);
			MetaCollectionPageHandle = PageHandle.Null;
			MetaCollectionNameIndexRootPageHandle = PageHandle.Null;
			FirstAllocationPageHandle = PageHandle.Null;
			LastAllocationPageHandle = PageHandle.Null;
		}

		public RootPage(PageBuffer buffer) : base(buffer)
		{
			var i = ReadBaseAndGetStartBufferOffset();
			var span = PageBuffer.AsSpan();

			FileMagic = HelpRead.AsUInt64(span[i..]);
			i += sizeof(ulong);
			BarbadosMagic = HelpRead.AsUInt64(span[i..]);
			i += sizeof(ulong);
			Version = HelpRead.AsUInt32(span[i..]);
			i += sizeof(uint);
			NextAvailablePageHandle = HelpRead.AsPageHandle(span[i..]);
			i += PageHandle.BinaryLength;
			MetaCollectionPageHandle = HelpRead.AsPageHandle(span[i..]);
			i += PageHandle.BinaryLength;
			MetaCollectionNameIndexRootPageHandle = HelpRead.AsPageHandle(span[i..]);
			i += PageHandle.BinaryLength;
			FirstAllocationPageHandle = HelpRead.AsPageHandle(span[i..]);
			i += PageHandle.BinaryLength;
			LastAllocationPageHandle = HelpRead.AsPageHandle(span[i..]);
			i += PageHandle.BinaryLength;

			Debug.Assert(Header.Marker == PageMarker.Root);
		}

		public void SetNextAvailablePageHandle(PageHandle handle)
		{
			if (!NextAvailablePageHandle.IsWithinBounds)
			{
				throw new BarbadosException(BarbadosExceptionCode.MaxPageCountReached);
			}

			NextAvailablePageHandle = handle;
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

			HelpWrite.AsUInt64(span[i..], FileMagic);
			i += sizeof(ulong);
			HelpWrite.AsUInt64(span[i..], BarbadosMagic);
			i += sizeof(ulong);
			HelpWrite.AsUInt32(span[i..], Version);
			i += sizeof(uint);
			HelpWrite.AsPageHandle(span[i..], NextAvailablePageHandle);
			i += PageHandle.BinaryLength;
			HelpWrite.AsPageHandle(span[i..], MetaCollectionPageHandle);
			i += PageHandle.BinaryLength;
			HelpWrite.AsPageHandle(span[i..], MetaCollectionNameIndexRootPageHandle);
			i += PageHandle.BinaryLength;
			HelpWrite.AsPageHandle(span[i..], FirstAllocationPageHandle);
			i += PageHandle.BinaryLength;
			HelpWrite.AsPageHandle(span[i..], LastAllocationPageHandle);
			i += PageHandle.BinaryLength;

			return PageBuffer;
		}
	}
}
