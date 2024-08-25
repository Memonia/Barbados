namespace Barbados.StorageEngine.Storage.Paging
{
	internal interface IOneWayChainPage
	{
		public PageHandle Next { get; set; }
	}
}
