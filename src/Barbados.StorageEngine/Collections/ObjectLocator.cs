using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Collections
{
	internal readonly struct ObjectLocator(ObjectId id, PageHandle handle)
	{
		public ObjectId Id { get; } = id;
		public PageHandle Handle { get; } = handle;
	}
}
