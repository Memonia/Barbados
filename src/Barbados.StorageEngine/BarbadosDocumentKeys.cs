using Barbados.Documents;

namespace Barbados.StorageEngine
{
	public static class BarbadosDocumentKeys
	{
		public static BarbadosKey DocumentId { get; } = $"{BarbadosDbObjects.ReservedNamePrefix}id";

		internal static class MetaCollection
		{
			public static BarbadosKey IndexArrayField { get; } = "i";
			public static BarbadosKey IndexDocumentPageHandleField { get; } = "ph";
			public static BarbadosKey IndexDocumentIndexedFieldField { get; } = "n";
			public static BarbadosKey CollectionDocumentField { get; } = "c";
			public static BarbadosKey CollectionDocumentNameField { get; } = "n";
			public static BarbadosKey CollectionDocumentPageHandleField { get; } = "ph";
			public static BarbadosKey CollectionDocumentIdGenModeField { get; } = "g";
			public static BarbadosKey AbsCollectionDocumentNameField { get; } = $"{CollectionDocumentField}{BarbadosKey.NestingSeparator}{CollectionDocumentNameField}";
			public static BarbadosKey AbsCollectionDocumentPageHandleField { get; } = $"{CollectionDocumentField}{BarbadosKey.NestingSeparator}{CollectionDocumentPageHandleField}";
			public static BarbadosKey AbsCollectionDocumentIdGenModeField { get; } = $"{CollectionDocumentField}{BarbadosKey.NestingSeparator}{CollectionDocumentIdGenModeField}";
		}
	}
}
