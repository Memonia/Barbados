namespace Barbados.StorageEngine
{
	public readonly struct ObjectId
	{
		internal const int BinaryLength = sizeof(long);

		public static ObjectId Invalid { get; } = new(0);
		public static ObjectId MaxValue { get; } = new(long.MaxValue);
		public static ObjectId MinValue { get; } = new(long.MinValue);

		public bool IsValid => Value != 0;

		public long Value { get; }

		public ObjectId(long id)
		{
			Value = id;
		}

		public override string ToString() => $"ObjectId({(!IsValid ? "n/a" : Value)})";
	}
}
