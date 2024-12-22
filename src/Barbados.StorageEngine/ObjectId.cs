using Barbados.Documents;

namespace Barbados.StorageEngine
{
	// TODO: temporary extension. Needs to be removed when indexes no longer depend on the primary
	// key having a specific type
	public static class BarbadosDocumentExtensions
	{
		public static ObjectId GetObjectId(this BarbadosDocument document)
		{
			return new(document.GetInt64(BarbadosDocumentKeys.DocumentId));
		}
	}

	public readonly struct ObjectId(long id)
	{
		public static ObjectId Invalid { get; } = new(0);
		public static ObjectId MaxValue { get; } = new(long.MaxValue);
		public static ObjectId MinValue { get; } = new(long.MinValue);

		public bool IsValid => Value != 0;

		public long Value { get; } = id;

		public override string ToString() => $"ObjectId({(!IsValid ? "n/a" : Value)})";
	}
}
