using System;
using System.Diagnostics;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		private ref struct BreadthFirstNoRootNodeEnumerator
		{
			/* Allocation-free breadth-first traversal implementation. 
			 * 
			 * We take advantage of the fact that nodes in the buffer are serialised in the breadth-first
			 * order. Each node's children are grouped together, separated by the 'IsLastChild' flag on each
			 * final child of the group. Groups of children of adjacent parents are also grouped together and
			 * the pattern repeats. 
			 * 
			 * We start on the first child of the given root node. As we enumerate current group, we count the
			 * number of nodes which have children. Once we hit the last node of the group, we move on to the
			 * first child of the first node which had children in the current group. We enumerate nodes just 
			 * like before, except once we hit the last node, we decrement the aforementioned counter and continue
			 * enumerating the next group. We repeat this process until the counter is zero and we hit a final node.
			 * Of course, as we were doing that, we counted the number of nodes with children, so that we can repeat
			 * the process on the next level. 
			 * 
			 * Eventually we will reach the final level, where no nodes will have children and that's where enumeration
			 * ends.
			 * 
			 * The algorithm doesn't include the root in its enumeration. Operations on the buffer mostly don't need the 
			 * root to be included, plus we reduce the number of edge-cases to handle
			 */

			private readonly ReadOnlySpan<byte> _buffer;
			private NodeGroupEnumerator _currentGroupEnumerator;
			private int _nextLevelFirstNodeOffset;
			private int _prevLevelNodeWithChildrenCount;
			private int _currentLevelNodeWithChildrenCount;

			public BreadthFirstNoRootNodeEnumerator(ReadOnlySpan<byte> buffer)
			{
				if (_isEmpty(buffer))
				{
					_currentGroupEnumerator = new(buffer, -1);
					return;
				}

				_buffer = buffer;
				// The root of the whole tree is not explicitly serialised,
				// so we can start enumerating from the first node
				_currentGroupEnumerator = new(buffer, PrefixTableOffset);
				_nextLevelFirstNodeOffset = -1;
				// See the overload
				_prevLevelNodeWithChildrenCount = 1;
				_currentLevelNodeWithChildrenCount = 0;
			}

			public BreadthFirstNoRootNodeEnumerator(ReadOnlySpan<byte> buffer, int rootOffset)
			{
				if (_isEmpty(buffer))
				{
					_currentGroupEnumerator = new(buffer, -1);
					return;
				}

				_buffer = buffer;

				// Set the offset to the first child of the root
				var pd = _getPrefixDescriptor(buffer, rootOffset);
				if (pd.HasChildren)
				{
					var offset = _getNodeFirstChildOffset(buffer, pd);
					_currentGroupEnumerator = new(buffer, offset);
				}

				// Or stop the enumeration immediately if the root has no children
				else
				{
					_currentGroupEnumerator = new(buffer, -1);
				}

				_nextLevelFirstNodeOffset = -1;
				// For the first level root is the only parent
				_prevLevelNodeWithChildrenCount = 1;
				_currentLevelNodeWithChildrenCount = 0;
			}

			public bool TryGetNext(out NodeInfo info)
			{
				// Will only happen on the last level.
				// Leaves don't have children and so '_nextLevelFirstNodeOffset' will not be set
				if (!_currentGroupEnumerator.TryGetNext(out info))
				{
					return false;
				}

				if (info.Descriptor.HasChildren)
				{
					// Only set the next level offset once per level
					if (_nextLevelFirstNodeOffset < 0)
					{
						Debug.Assert(_currentLevelNodeWithChildrenCount == 0);
						_nextLevelFirstNodeOffset = _getNodeFirstChildOffset(_buffer, info.Descriptor);
					}

					_currentLevelNodeWithChildrenCount += 1;
				}

				if (info.Descriptor.IsLastChild)
				{
					_prevLevelNodeWithChildrenCount -= 1;

					// Move on to the next level
					if (_prevLevelNodeWithChildrenCount == 0)
					{
						_currentGroupEnumerator = new(_buffer, _nextLevelFirstNodeOffset);
						_nextLevelFirstNodeOffset = -1;
						_prevLevelNodeWithChildrenCount = _currentLevelNodeWithChildrenCount;
						_currentLevelNodeWithChildrenCount = 0;
					}

					// Move on to the next group
					else
					{
						var offset = _getNextNodeOffset(_buffer, info);
						_currentGroupEnumerator = new(_buffer, offset);
					}
				}

				return true;
			}
		}
	}
}
