namespace Barbados.StorageEngine
{
	public static class BarbadosDbObjects
	{
		public static string ReservedNamePrefix { get; } = "?";

		public static class Collections
		{
			public static readonly BarbadosDbObjectName MetaCollection = $"{ReservedNamePrefix}meta";
		}
	}
}
