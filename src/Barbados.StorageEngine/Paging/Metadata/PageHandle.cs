namespace Barbados.StorageEngine.Paging.Metadata;

internal readonly partial struct PageHandle(long index)
{
	public static readonly PageHandle Null = new(0);
	public static readonly PageHandle Root = new(1);

	public bool IsNull => Index == Null.Index;
	public bool IsWithinBounds => Index < (long.MaxValue / Constants.PageLength);

	public long Index { get; } = index;

	public long GetAddress()
	{
		DEBUG_ThrowNullHandleDereference();
		DEBUG_ThrowInvalidHandleDereference();
		DEBUG_ThrowOutOfBoundsHandleDereference();
		return (Index - 1) * Constants.PageLength;
	}

	public override string ToString()
	{
		return $"PageHandle({Index})";
	}
}
