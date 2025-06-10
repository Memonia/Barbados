namespace Barbados.StorageEngine.Collections
{
	public sealed class CreateCollectionOptions
	{
		public static CreateCollectionOptions Default { get; } = new CreateCollectionOptions();

		public bool UniquePrimaryKey { get; } = true;
		public AutomaticIdGeneratorMode AutomaticIdGeneratorMode { get; init; } = AutomaticIdGeneratorMode.BetterSpaceUtilisation;
	}
}
