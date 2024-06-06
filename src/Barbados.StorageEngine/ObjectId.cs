namespace Barbados.StorageEngine
{
	public readonly struct ObjectId(long id)
	{
		public static ObjectId Invalid { get; } = new(0);
		public static ObjectId MaxValue { get; } = new(long.MaxValue);

		public bool IsValid => Value != 0;

		public long Value { get; } = id;

		public override string ToString() => $"ObjectId({(!IsValid ? "n/a" : Value)})";
	}
}
