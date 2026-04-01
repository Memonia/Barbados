using System.Collections.Generic;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		private partial class PrivateEnums
		{
			public readonly ref struct DepthFirstNodesAccumulator
			{
				public readonly List<NodeInfo> EnumerateCurrentPathTopBottom() => _accumulator;

				private readonly List<NodeInfo> _accumulator;
				private readonly PrivateEnums.DepthFirstNodes _enum;

				public DepthFirstNodesAccumulator(PrivateEnums.DepthFirstNodes dfne)
				{
					_enum = dfne;
					_accumulator = [];
				}

				public bool TryContinueUntilNodeWithValue()
				{
					while (_tryContinue(out var node))
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
					var relativeNodeDepth = info.Depth - _enum.InitialDepth + NodeInfoDepth.FirstLevelDepth;
					var currentAccumulatedRelativeDepth = _accumulator.Count + 1;
					if (relativeNodeDepth < currentAccumulatedRelativeDepth)
					{
						var diff = currentAccumulatedRelativeDepth - relativeNodeDepth;
						_accumulator.RemoveRange(_accumulator.Count - diff, diff);
					}

					_accumulator.Add(info.NodeInfo);
					return true;
				}
			}
		}
	}
}
