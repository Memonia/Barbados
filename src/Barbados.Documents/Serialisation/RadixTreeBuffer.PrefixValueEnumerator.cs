using System;
using System.Diagnostics;

using Barbados.Documents.Serialisation.Metadata;
using Barbados.Documents.Serialisation.Values;

namespace Barbados.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		public readonly ref struct PrefixValueEnumerator
		{
			private readonly ReadOnlySpan<byte> _buffer;
			private readonly DepthFirstNodeAccumulator _accumulator;

			public PrefixValueEnumerator(RadixTreeBuffer buffer) : this(buffer._buffer)
			{

			}

			public PrefixValueEnumerator(ReadOnlySpan<byte> buffer)
			{
				_buffer = buffer;
				_accumulator = new(new DepthFirstNodeEnumerator(buffer));
			}

			public bool TryGetNext(out RadixTreePrefix prefix)
			{
				if (_tryGetNextNode(out var prefixBytes, out _))
				{
					prefix = new RadixTreePrefix(prefixBytes);
					return true;
				}

				prefix = default!;
				return false;
			}

			public bool TryGetNext(out RadixTreePrefix prefix, out IValueBuffer valueBuffer)
			{
				if (_tryGetNextNode(out var prefixBytes, out var node))
				{
					Debug.Assert(node.Descriptor.HasValue);
					var vd = _getValueDescriptor(_buffer, node.Offset + PrefixDescriptor.BinaryLength);
					var vb = _getValueBuffer(_buffer, vd);

					prefix = new RadixTreePrefix(prefixBytes);
					valueBuffer = ValueBufferFactory.CreateFromRawBuffer(vb, vd.Marker);
					return true;
				}

				prefix = default!;
				valueBuffer = default!;
				return false;
			}

			private bool _tryGetNextNode(out byte[] prefix, out NodeInfo node)
			{
				if (!_accumulator.TryContinueUntilNodeWithValue(out node))
				{
					prefix = default!;
					node = default!;
					return false;
				}

				var prefixLength = 0;
				foreach (var info in _accumulator.EnumerateCurrentPathTopBottom())
				{
					var part = _getNodePrefix(_buffer, info);
					prefixLength += part.Length;
				}

				var i = 0;
				prefix = new byte[prefixLength];
				var pspan = prefix.AsSpan();
				foreach (var info in _accumulator.EnumerateCurrentPathTopBottom())
				{
					var part = _getNodePrefix(_buffer, info);
					part.CopyTo(pspan[i..]);
					i += part.Length;
				}

				return true;
			}
		} 
	}
}
