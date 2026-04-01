namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		private readonly struct NodeInfoDepth
		{
			public static int FirstLevelDepth { get; } = 1;

			public int Depth { get; }
			public NodeInfo NodeInfo { get; }

			public NodeInfoDepth(int depth, NodeInfo info)
			{
				Depth = depth;
				NodeInfo = info;
			}
		}
	}
}
