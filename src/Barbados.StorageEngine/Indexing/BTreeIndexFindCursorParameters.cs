using Barbados.StorageEngine.Indexing.Search;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed partial class BTreeIndexFindCursorParameters
	{
		public BTreeIndexFindCursorParameters(
			long take,
			bool takeAll,
			bool ascending,
			KeyCheckRange check
		)
		{
			Take = take;
			TakeAll = takeAll;
			Ascending = ascending;
			Check = check;
		}

		public long Take { get; }
		public bool TakeAll { get; }
		public bool Ascending { get; }
		public KeyCheckRange Check { get; }
	}
}
