namespace Barbados.StorageEngine
{
	public static class CommonIdentifiers
	{
		public static char NestingSeparator { get; } = '.';
		public static string ReservedNamePrefix { get; } = "?";
		public static BarbadosIdentifier Id { get; } = "?id";

		public static class Index
		{
			public static readonly BarbadosIdentifier SearchValue = "sval";
			public static readonly BarbadosIdentifier Take = "take";
			public static readonly BarbadosIdentifier Inclusive = "incl";
			public static readonly BarbadosIdentifier Ascending = "asc";
			public static readonly BarbadosIdentifier Exact = "eq";
			public static readonly BarbadosIdentifier Range = "rg";
			public static readonly BarbadosIdentifier LessThan = "lt";
			public static readonly BarbadosIdentifier GreaterThan = "gt";
		}

		public static class Collections
		{
			public static readonly BarbadosIdentifier MetaCollection = "?meta";
		}

		internal static class MetaCollection
		{
			public static readonly BarbadosIdentifier IndexArrayField = "i";
			public static readonly BarbadosIdentifier IndexDocumentPageHandleField = "ph";
			public static readonly BarbadosIdentifier IndexDocumentIndexedFieldField = "n";
			public static readonly BarbadosIdentifier IndexDocumentKeyMaxLengthField = "l";
			public static readonly BarbadosIdentifier CollectionDocumentField = "c";
			public static readonly BarbadosIdentifier CollectionDocumentNameField = "n";
			public static readonly BarbadosIdentifier CollectionDocumentPageHandleField = "ph";
			public static readonly BarbadosIdentifier AbsCollectionDocumentNameField = "c.n";
			public static readonly BarbadosIdentifier AbsCollectionDocumentPageHandleField = "c.ph";
		}
	}
}
