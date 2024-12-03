using System;
using System.Collections.Generic;
using System.Text;

using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal sealed class RadixTreeNode
	{
		/* A radix tree implementation tailored for key-value accumulation during document creation
		 * and subsequent serialisation
		 */

		public IValueBuffer? Value { get; private set; }

		// SortedList could be used here to enable binary search, but
		// each node is expected to have little children to justify the overhead
		private readonly List<KeyValuePair<RadixTreePrefix, RadixTreeNode>> _children;

		public RadixTreeNode() : this(null)
		{

		}

		private RadixTreeNode(IValueBuffer? value)
		{
			Value = value;
			_children = [];
		}

		public IEnumerable<RadixTreeNodeInfo> EnumerateNodesBreadthFirst()
		{
			var queue = new Queue<(RadixTreeNode, RadixTreeNodeInfo)>();

			// root node must not be serialised anyways, so the values do not matter
			queue.Enqueue(new(this, new(RadixTreePrefix.Empty, null, -1, false, false)));

			var count = -1;
			while (queue.Count > 0)
			{
				var (node, info) = queue.Dequeue();
				if (node._children.Count > 0)
				{
					info.FirstChildId = count + 1;
				}

				var remainingChildren = node._children.Count;
				foreach (var (prefix, child) in node._children)
				{
					count += 1;
					remainingChildren -= 1;
					queue.Enqueue(
						new(child,
							new(
								prefix: prefix,
								value: child.Value,
								nodeId: count,
								isLastChild: remainingChildren == 0,
								isFirstChild: remainingChildren == node._children.Count - 1
							)
						)
					);
				}

				yield return info;
			}
		}

		public bool TryGet(RadixTreePrefixSpan key, out IValueBuffer value)
		{
			// The tree is not empty-string terminated, so if the key is empty at this point in search,
			// current node is the only place where the value can be.
			//
			// The search should start from the root of the tree, where the value is always null.
			// If an empty string was given, then there's no match anyways and null is to be expected
			if (key.Length == 0)
			{
				value = Value!;
				return value is not null;
			}

			foreach (var (prefix, child) in _children)
			{
				if (key.StartsWith(prefix))
				{
					return child.TryGet(key[prefix.Length..], out value);
				}
			}

			value = default!;
			return false;
		}

		public void Add(RadixTreePrefixSpan key, IValueBuffer? value)
		{
			var index = -1;
			foreach (var (prefix, child) in _children)
			{
				index += 1;
				var cpl = key.CommonPrefixLength(prefix);
				if (cpl == 0)
				{
					continue;
				}

				// If the key contains the current prefix we can either proceed down the tree or stop here  
				if (cpl == prefix.Length)
				{
					// If the key is of the length of the prefix - we have a match
					if (cpl == key.Length)
					{
						// If current child has a value - the key is a duplicate
						if (child.Value is not null)
						{
							throw new ArgumentException($"Duplicate key {key.ToString()}");
						}

						child.Value = value;
					}

					// Key only contains the prefix - we proceed with the search down the tree
					else
					{
						child.Add(key[prefix.Length..], value);
					}
				}

				// Key contains a part of the prefix. We know that this prefix is the best match, because all previous
				// partial matches have resulted in a split. The split point will be the shared prefix of the key and
				// the current prefix. Current subtree will go under the right-side portion of the prefix split, because
				// that's the original path of the current prefix. The new value will go under the right-side portion of
				// the key split, because that's the new diverging path
				else
				{
					// Can be taken from either the key or the prefix 
					var splitPrefix = prefix[..cpl];

					// Split node will have two paths, sprouting from the shared prefix
					var keyRightSplit = key[cpl..];
					var prefixRightSplit = prefix[cpl..];

					var split = new RadixTreeNode();

					_children.RemoveAt(index);
					_children.Add(new(splitPrefix, split));
					split._children.Add(new(prefixRightSplit, child));

					// It could be that the key was a prefix for the current prefix, in which case
					// the search stops at the split point. The tree is not empty-string terminated
					if (keyRightSplit.Length == 0)
					{
						split.Value = value;
					}

					else
					{
						split.Add(keyRightSplit, value);
					}
				}

				return;
			}

			// See 'RadixTreeBuffer'. A prefix cannot exceed a certain length for serialisation purposes.
			// In order to support arbitrary key lengths, long prefixes are split across the tree levels.
			// It can get inefficient if the keys are long and span multiple levels, but it shouldn't be
			// the case in most real-world scenarios
			if (key.Length > RadixTreeBuffer.MaxNodePrefixLength)
			{
				var node = new RadixTreeNode();
				var keyLeftCut = key[..RadixTreeBuffer.MaxNodePrefixLength];
				var keyRightCut = key[RadixTreeBuffer.MaxNodePrefixLength..];

				_children.Add(new(new(keyLeftCut), node));

				// The remainder of the key can be added as-is. Whenever '_children' collection is modified,
				// the prefix associated with the child is always left-hand side of whatever key was added or
				// the right-hand side of an existing prefix, which is guaranteed to be split by this code path
				node.Add(keyRightCut, value);
			}

			else
			{
				var node = new RadixTreeNode(value);
				_children.Add(new(new(key), node));
			}
		}

		public string GetTree()
		{
			var sb = new StringBuilder();

			_getTree(sb, 0);
			return sb.ToString();
		}

		private void _getTree(StringBuilder sb, int depth)
		{
			sb.Append(' ');
			sb.Append(Value?.ToString() ?? "<none>");
			sb.AppendLine();
			foreach (var (prefix, child) in _children)
			{
				sb.Append(' ', depth);
				sb.Append('|');
				sb.Append(prefix);
				child._getTree(sb, depth + 1);
			}
		}
	}
}
