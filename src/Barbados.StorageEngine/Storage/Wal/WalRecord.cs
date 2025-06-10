using System;

namespace Barbados.StorageEngine.Storage.Wal
{
	internal readonly struct WalRecord
	{
		public uint Checksum { get; }
		public ObjectId TransactionId { get; }
		public WalRecordTypeMarker Marker { get; }

		public WalRecord(ReadOnlySpan<byte> record)
		{
			var i = 0;
			Checksum = HelpRead.AsUInt32(record[i..]);
			i += sizeof(uint);
			TransactionId = HelpRead.AsObjectId(record[i..]);
			i += ObjectId.BinaryLength;
			Marker = (WalRecordTypeMarker)record[i];
			i += sizeof(byte);
		}

		public WalRecord(WalRecordTypeMarker marker, ObjectId transactionId)
		{
			TransactionId = transactionId;
			Marker = marker;
		}

		public void WriteTo(Span<byte> destination)
		{
			var i = sizeof(uint);
			HelpWrite.AsObjectId(destination[i..], TransactionId);
			i += ObjectId.BinaryLength;
			destination[i] = (byte)Marker;
			i += sizeof(byte);

			var checksum = _checksum();
			HelpWrite.AsUInt32(destination, checksum);
		}

		public bool VerifyChecksum()
		{
			return Checksum == _checksum();
		}

		private uint _checksum()
		{
			var checksum = uint.MaxValue;
			checksum = Crc32.Combine(checksum, (byte)Marker);
			checksum = Crc32.Combine(checksum, (ulong)TransactionId.Value);
			return checksum;
		}
	}
}
