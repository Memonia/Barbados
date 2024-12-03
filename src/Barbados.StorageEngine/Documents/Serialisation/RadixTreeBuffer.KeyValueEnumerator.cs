using System;
using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Documents.Serialisation.Metadata;
using Barbados.StorageEngine.Documents.Serialisation.Values;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		public readonly ref struct KeyValueEnumerator
		{
			private readonly ReadOnlySpan<byte> _buffer;
			private readonly RadixTreePrefixDecoder _accumulator;
			private readonly DepthFirstNodeEnumerator _enum;
			private readonly List<NodeInfo> _pathNodes;

			public KeyValueEnumerator(RadixTreeBuffer buffer) : this(buffer._buffer)
			{
				
			}

			public KeyValueEnumerator(ReadOnlySpan<byte> buffer)
			{
				_buffer = buffer;
				_accumulator = new();
				_enum = new(buffer);
				_pathNodes = [];
			}

			public bool TryGetNext(out ReadOnlySpan<char> key)
			{
				if (_tryGetNextNode(out _))
				{
					key = _accumulator.GetCharArrayAndReturn();
					return true;
				}

				key = default!;
				return false;
			}

			public bool TryGetNext(out ReadOnlySpan<char> key, out IValueBuffer valueBuffer)
			{
				if (_tryGetNextNode(out var node))
				{
					Debug.Assert(node.Descriptor.HasValue);
					var vd = _getValueDescriptor(_buffer, node.Offset + PrefixDescriptor.BinaryLength);
					var vb = _getValueBuffer(_buffer, vd);

					key = _accumulator.GetCharArrayAndReturn();
					valueBuffer = ValueBufferFactory.CreateFromRawBuffer(vb, vd.Marker);
					return true;
				}

				key = default!;
				valueBuffer = default!;
				return false;

			}

			private bool _tryGetNextNode(out NodeInfo node)
			{
				while (true)
				{
					if (!_enum.TryGetNext(out var info))
					{
						node = default!;
						return false;
					}

					node = info.NodeInfo;
					if (info.Depth <= _pathNodes.Count)
					{
						var diff = _pathNodes.Count - info.Depth + 1;
						_pathNodes.RemoveRange(_pathNodes.Count - diff, diff);
					}

					_pathNodes.Add(info.NodeInfo);
					if (info.NodeInfo.Descriptor.HasValue)
					{
						var charCount = 0;
						for (int i = 0; i < _pathNodes.Count; ++i)
						{
							var part = _getNodePrefix(_buffer, _pathNodes[i]);
							charCount += _accumulator.GetRunningCharCount(part);
						}

						_accumulator.Reset();
						_accumulator.CreateCharBuffer(charCount);
						for (int i = 0; i < _pathNodes.Count; ++i)
						{
							var part = _getNodePrefix(_buffer, _pathNodes[i]);
							_accumulator.AppendCharsFrom(part);
						}

						break;
					}
				}

				return true;
			}
		}
	}
}
