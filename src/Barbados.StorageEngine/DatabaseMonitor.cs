using Barbados.StorageEngine.Collections;

namespace Barbados.StorageEngine
{
	internal sealed partial class DatabaseMonitor : IDatabaseMonitor
	{
		private readonly MetaCollectionFacade _meta;

		public DatabaseMonitor(MetaCollectionFacade meta)
		{
			_meta = meta;
		}
	}
}
