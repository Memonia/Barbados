using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Paging
{
	internal interface IOneWayChainPage
	{
		public PageHandle Next { get; set; }
	}
}
