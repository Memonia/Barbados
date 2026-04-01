namespace Barbados.StorageEngine.Storage.Paging
{
	internal readonly partial struct PageHandle
	{
		public const int BinaryLength = sizeof(long);

		public static PageHandle Null { get; } = new(0);
		public static PageHandle Root { get; } = new(1);

		public bool IsNull => Handle == Null.Handle;
		public bool IsWithinBounds => Handle < (long.MaxValue / Constants.PageLength);

		public long Handle { get; }

		public PageHandle(long handle)
		{
			Handle = handle;
		}

		public long GetAddress()
		{
			DEBUG_ThrowNullHandleDereference();
			DEBUG_ThrowInvalidHandleDereference();
			DEBUG_ThrowOutOfBoundsHandleDereference();
			return (Handle - 1) * Constants.PageLength;
		}

		public override string ToString()
		{
			return $"PageHandle({Handle})";
		}
	}
}
