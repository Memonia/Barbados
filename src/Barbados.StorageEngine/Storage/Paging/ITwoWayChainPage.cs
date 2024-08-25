namespace Barbados.StorageEngine.Storage.Paging
{
	internal interface ITwoWayChainPage : IOneWayChainPage
	{
		public new PageHandle Next { get; set; }
		public PageHandle Previous { get; set; }
	}
}
