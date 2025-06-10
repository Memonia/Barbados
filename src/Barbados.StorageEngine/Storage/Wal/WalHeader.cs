using System;

using Barbados.StorageEngine.Exceptions;

namespace Barbados.StorageEngine.Storage.Wal
{
	internal sealed class WalHeader
	{
		public static void ThrowWalDoesNotExist(byte[] header)
		{
			var magic = HelpRead.AsUInt64(header.AsSpan()[sizeof(ulong)..]);
			if (magic != Constants.WalMagicNumber)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.WalDoesNotExist, "Could not validate the WAL file"
				);
			}
		}

		public static void ThrowWalVersionMismatch(byte[] header)
		{
			var span = header.AsSpan();
			var version = HelpRead.AsUInt32(span[(sizeof(ulong) * 2)..]);
			if (version != Constants.WalVersion)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.WalVersionMismatch,
					$"Unexpected WAL file version. Expected version {Constants.WalVersion}, found version {version}"
				);
			}
		}

		public ulong FileMagic { get; }
		public ulong BarbadosMagic { get; }

		public uint Version { get; }

		public WalHeader(ulong fileMagic)
		{
			FileMagic = fileMagic;
			BarbadosMagic = Constants.WalMagicNumber;
			Version = Constants.WalVersion;
		}

		public WalHeader(byte[] header)
		{
			var span = header.AsSpan();
			var i = 0;

			FileMagic = HelpRead.AsUInt64(span[i..]);
			i += sizeof(ulong);
			BarbadosMagic = HelpRead.AsUInt64(span[i..]);
			i += sizeof(ulong);
			Version = HelpRead.AsUInt32(span[i..]);
			i += sizeof(uint);
		}

		public void WriteTo(Span<byte> destination)
		{
			var i = 0;
			HelpWrite.AsUInt64(destination[i..], FileMagic);
			i += sizeof(ulong);
			HelpWrite.AsUInt64(destination[i..], BarbadosMagic);
			i += sizeof(ulong);
			HelpWrite.AsUInt32(destination[i..], Version);
			i += sizeof(uint);
		}
	}
}
