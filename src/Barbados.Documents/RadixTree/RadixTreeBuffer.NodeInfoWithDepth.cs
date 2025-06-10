namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		private readonly struct NodeInfoWithDepth
		{
			public int Depth { get; }
			public NodeInfo NodeInfo { get; }

			public NodeInfoWithDepth(int depth, NodeInfo info)
			{
				Depth = depth;
				NodeInfo = info;
			}
		}
	}
}
