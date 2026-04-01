using System;
using System.Collections.Generic;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		private partial class PrivateEnums
		{
			public readonly ref struct DepthFirstNodes
			{
				public int InitialDepth { get; }

				private readonly ReadOnlySpan<byte> _buffer;
				private readonly Stack<NodeInfoDepth> _nodeStack;

				public DepthFirstNodes(ReadOnlySpan<byte> buffer)
				{
					_buffer = buffer;
					_nodeStack = new();

					if (!IsEmpty(buffer))
					{
						var e = new BreadthFirstSingleLevel(buffer, PrefixTableOffset);
						while (e.TryGetNext(out var info))
						{
							_nodeStack.Push(new(NodeInfoDepth.FirstLevelDepth, info));
						}
					}

					InitialDepth = NodeInfoDepth.FirstLevelDepth;
				}

				public DepthFirstNodes(ReadOnlySpan<byte> buffer, int rootOffset, bool includeRoot)
				{
					_buffer = buffer;
					_nodeStack = [];

					if (!IsEmpty(buffer))
					{
						var pd = _getPrefixDescriptor(buffer, rootOffset);
						_nodeStack.Push(new(NodeInfoDepth.FirstLevelDepth, new NodeInfo(rootOffset, pd)));
					}

					if (!includeRoot)
					{
						TryGetNext(out _);
					}

					if (_nodeStack.TryPeek(out var info))
					{
						InitialDepth = info.Depth;
					}

					else
					{
						InitialDepth = NodeInfoDepth.FirstLevelDepth;
					}
				}

				public bool TryGetNext(out NodeInfoDepth info)
				{
					if (!_nodeStack.TryPop(out info))
					{
						return false;
					}

					if (info.NodeInfo.Descriptor.HasChildren)
					{
						var offset = _getNodeFirstChildOffset(_buffer, info.NodeInfo.Descriptor);
						var e = new BreadthFirstSingleLevel(_buffer, offset);
						while (e.TryGetNext(out var ci))
						{
							_nodeStack.Push(new(info.Depth + 1, ci));
						}
					}

					return true;
				}
			}
		}
	}
}
