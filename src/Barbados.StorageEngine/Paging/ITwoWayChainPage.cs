using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging
{
	internal interface ITwoWayChainPage : IOneWayChainPage
	{
		public new PageHandle Next { get; set; }
		public PageHandle Previous { get; set; }
	}
}
