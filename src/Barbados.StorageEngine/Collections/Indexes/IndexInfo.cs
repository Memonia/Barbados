using Barbados.Documents;
using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Collections.Indexes
{
	internal sealed class IndexInfo
	{
		public BarbadosKey Field { get; }
		public BTreeInfo BTreeInfo { get; }

		public IndexInfo(BTreeInfo btreeInfo, BarbadosKey field)
		{
			Field = field;
			BTreeInfo = btreeInfo;
		}
	}
}
