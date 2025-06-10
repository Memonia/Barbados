using Barbados.Documents;

namespace Barbados.StorageEngine.Collections
{
	public interface IReadOnlyBarbadosCollection
	{
		ObjectId Id { get; }
		BarbadosDbObjectName Name { get; }
		AutomaticIdGeneratorMode AutomaticIdGeneratorMode { get; }

		bool IndexExists(BarbadosKey field);

		ICursor<BarbadosDocument> Find(FindOptions options);
		ICursor<BarbadosDocument> Find(FindOptions options, BarbadosKey indexField);
	}
}
