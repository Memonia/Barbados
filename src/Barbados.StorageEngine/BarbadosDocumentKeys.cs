using Barbados.Documents;

namespace Barbados.StorageEngine
{
	public static class BarbadosDocumentKeys
	{
		public static BarbadosKey DocumentId { get; } = $"{BarbadosDbObjects.ReservedNamePrefix}id";

		public static class IndexQuery
		{
			public static readonly BarbadosKey SearchValue = "sval";
			public static readonly BarbadosKey Take = "take";
			public static readonly BarbadosKey Inclusive = "incl";
			public static readonly BarbadosKey Ascending = "asc";
			public static readonly BarbadosKey Exact = "eq";
			public static readonly BarbadosKey Range = "rg";
			public static readonly BarbadosKey LessThan = "lt";
			public static readonly BarbadosKey GreaterThan = "gt";
		}

		internal static class MetaCollection
		{
			public static readonly BarbadosKey IndexArrayField = "i";
			public static readonly BarbadosKey IndexDocumentPageHandleField = "ph";
			public static readonly BarbadosKey IndexDocumentIndexedFieldField = "n";
			public static readonly BarbadosKey IndexDocumentKeyMaxLengthField = "l";
			public static readonly BarbadosKey CollectionDocumentField = "c";
			public static readonly BarbadosKey CollectionDocumentNameField = "n";
			public static readonly BarbadosKey CollectionDocumentPageHandleField = "ph";
			public static readonly BarbadosKey AbsCollectionDocumentNameField = "c.n";
			public static readonly BarbadosKey AbsCollectionDocumentPageHandleField = "c.ph";
		}
	}
}
