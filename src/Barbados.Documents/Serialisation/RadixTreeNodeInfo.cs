using Barbados.Documents.Serialisation.Values;

namespace Barbados.Documents.Serialisation
{
	internal sealed class RadixTreeNodeInfo
	{
		public int NodeId { get; }
		public int FirstChildId { get; set; }
		public bool IsLastChild { get; }
		public bool IsFirstChild { get; }
		public IValueBuffer? Value { get; }
		public RadixTreePrefix Prefix { get; }

		public bool HasChildren => FirstChildId > 0;

		public RadixTreeNodeInfo(
			RadixTreePrefix prefix,
			IValueBuffer? value,
			int nodeId,
			bool isLastChild,
			bool isFirstChild
		)
		{
			FirstChildId = -1;
			NodeId = nodeId;
			Value = value;
			Prefix = prefix;
			IsLastChild = isLastChild;
			IsFirstChild = isFirstChild;
		}
	}
}
