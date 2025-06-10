using System.Collections.Generic;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		private readonly ref struct DepthFirstNodeAccumulator
		{
			public List<NodeInfo> EnumerateCurrentPathTopBottom() => _accumulator;

			private readonly List<NodeInfo> _accumulator;
			private readonly DepthFirstNodeEnumerator _enum;

			public DepthFirstNodeAccumulator(DepthFirstNodeEnumerator dfne)
			{
				_enum = dfne;
				_accumulator = [];
			}

			public bool TryContinueUntilNodeWithValue(out NodeInfo node)
			{
				while (_tryContinue(out node))
				{
					if (node.Descriptor.HasValue)
					{
						return true;
					}
				}

				return false;
			}

			private bool _tryContinue(out NodeInfo currentNode)
			{
				if (!_enum.TryGetNext(out var info))
				{
					currentNode = default!;
					return false;
				}

				currentNode = info.NodeInfo;
				var count = _accumulator.Count;
				if (info.Depth <= count)
				{
					var diff = count - info.Depth + 1;
					_accumulator.RemoveRange(count - diff, diff);
				}

				_accumulator.Add(info.NodeInfo);
				return true;
			}
		}
	}
}
