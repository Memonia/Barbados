using System;
using System.Collections.Generic;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		private readonly ref struct DepthFirstNodeEnumerator
		{
			private readonly ReadOnlySpan<byte> _buffer;
			private readonly Stack<NodeInfoWithDepth> _nodeStack;

			public DepthFirstNodeEnumerator(ReadOnlySpan<byte> buffer)
			{
				_buffer = buffer;
				_nodeStack = new();

				if (!_isEmpty(buffer))
				{
					var e = new NodeGroupEnumerator(buffer, PrefixTableOffset);
					while (e.TryGetNext(out var info))
					{
						_nodeStack.Push(new(1, info));
					}
				}
			}

			public DepthFirstNodeEnumerator(ReadOnlySpan<byte> buffer, int rootOffset)
			{
				_buffer = buffer;
				_nodeStack = [];

				if (!_isEmpty(buffer))
				{
					var pd = _getPrefixDescriptor(buffer, rootOffset);
					_nodeStack.Push(new(1, new NodeInfo(rootOffset, pd)));
				}
			}

			public bool TryGetNext(out NodeInfoWithDepth info)
			{
				if (!_nodeStack.TryPop(out info))
				{
					return false;
				}

				if (info.NodeInfo.Descriptor.HasChildren)
				{
					var offset = _getNodeFirstChildOffset(_buffer, info.NodeInfo.Descriptor);
					var e = new NodeGroupEnumerator(_buffer, offset);
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
