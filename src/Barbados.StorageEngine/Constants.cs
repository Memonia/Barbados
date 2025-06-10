namespace Barbados.StorageEngine
{
	internal static class Constants
	{
		public const uint DbVersion = 1;
		public const uint WalVersion = 1;
		public const ulong DbMagicNumber = 0x534F444142524142;
		public const ulong WalMagicNumber = 0x4C41572D534F4442;

		public const int PageLength = 4096;
		public const int WalHeaderLength = sizeof(ulong) + sizeof(ulong) + sizeof(uint);
		public const int WalRecordLength = sizeof(uint) + ObjectId.BinaryLength + sizeof(byte);
	}
}
