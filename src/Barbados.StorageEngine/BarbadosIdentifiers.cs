namespace Barbados.StorageEngine
{
	public static class BarbadosIdentifiers
	{
		public static readonly string NestingSeparator = ".";
		public static readonly BarbadosIdentifier PrefixInternal = "?";

		public static class Index
		{
			public static readonly BarbadosIdentifier SearchValue = "?sval";
			public static readonly BarbadosIdentifier Exact = "?eq";
			public static readonly BarbadosIdentifier Range = "?range";
			public static readonly BarbadosIdentifier LessThan = "?lt";
			public static readonly BarbadosIdentifier GreaterThan = "?gt";
			public static readonly BarbadosIdentifier Inclusive = "?incl";
		}

		public static class Collection
		{
			public static readonly BarbadosIdentifier MetaCollection = "?meta";
			public static readonly BarbadosIdentifier MetaCollectionIndex = "?metai";
		}

		internal static class MetaCollection
		{
			public static readonly BarbadosIdentifier IndexArrayField = "?i";
			public static readonly BarbadosIdentifier IndexDocumentPageHandleField = "?ph";
			public static readonly BarbadosIdentifier IndexDocumentIndexedFieldField = "?n";
			public static readonly BarbadosIdentifier IndexDocumentKeyMaxLengthField = "?l";

			public static readonly BarbadosIdentifier CollectionDocumentField = "?c";
			public static readonly BarbadosIdentifier CollectionDocumentNameFIeld = "?n";
			public static readonly BarbadosIdentifier CollectionDocumentPageHandleField = "?ph";
			public static readonly BarbadosIdentifier CollectionDocumentClusteredIndexPageHandleField = "?ciph";
			public static readonly BarbadosIdentifier CollectionDocumentNameFieldAbsolute = "?c.?n";
			public static readonly BarbadosIdentifier CollectionDocumentPageHandleFieldAbsolute = "?c.?ph";
			public static readonly BarbadosIdentifier CollectionDocumentClusteredIndexPageHandleFieldAbsolute = "?c.?ciph";
		}
	}
}
