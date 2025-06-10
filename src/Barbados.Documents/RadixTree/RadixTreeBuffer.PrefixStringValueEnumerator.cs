using System;
using System.Diagnostics;

using Barbados.Documents.RadixTree.Metadata;
using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		public readonly ref struct PrefixStringValueEnumerator
		{
			private readonly ReadOnlySpan<byte> _buffer;
			private readonly RadixTreePrefixDecoder _decoder;
			private readonly DepthFirstNodeAccumulator _accumulator;

			public PrefixStringValueEnumerator(RadixTreeBuffer buffer) : this(buffer._buffer)
			{

			}

			public PrefixStringValueEnumerator(ReadOnlySpan<byte> buffer)
			{
				_buffer = buffer;
				_decoder = new();
				_accumulator = new(new DepthFirstNodeEnumerator(buffer));
			}

			public bool TryGetNext(out string prefix)
			{
				if (_tryGetNextNode(out _))
				{
					prefix = _decoder.CreateStringAndReset();
					return true;
				}

				prefix = default!;
				return false;
			}

			public bool TryGetNext(out string prefix, out IValueBuffer valueBuffer)
			{
				if (_tryGetNextNode(out var node))
				{
					Debug.Assert(node.Descriptor.HasValue);
					var vd = _getValueDescriptor(_buffer, node.Offset + PrefixDescriptor.BinaryLength);
					var vb = _getValueBuffer(_buffer, vd);

					prefix = _decoder.CreateStringAndReset();
					valueBuffer = ValueBufferFactory.CreateFromRawBuffer(vb, vd.Marker);
					return true;
				}

				prefix = default!;
				valueBuffer = default!;
				return false;
			}

			private bool _tryGetNextNode(out NodeInfo node)
			{
				if (!_accumulator.TryContinueUntilNodeWithValue(out node))
				{
					node = default!;
					return false;
				}

				_decoder.Reset();
				foreach (var info in _accumulator.EnumerateCurrentPathTopBottom())
				{
					var part = _getNodePrefix(_buffer, info);
					_decoder.AppendCharsFrom(part);
				}

				return true;
			}
		}
	}
}
