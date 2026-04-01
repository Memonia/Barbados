using System;
using System.Diagnostics;
using System.Linq;

using Barbados.Documents.RadixTree.Metadata;
using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		public readonly ref struct FullPrefixHasValueEnumerator
		{
			private readonly bool _stop;
			private readonly ReadOnlySpan<byte> _buffer;
			private readonly PrivateEnums.DepthFirstNodesAccumulator _accumulator;

			public FullPrefixHasValueEnumerator(RadixTreeBuffer buffer) : this(buffer._buffer)
			{

			}

			public FullPrefixHasValueEnumerator(ReadOnlySpan<byte> buffer)
			{
				_stop = false;
				_buffer = buffer;
				_accumulator = new(new PrivateEnums.DepthFirstNodes(buffer));
			}

			public FullPrefixHasValueEnumerator(ReadOnlySpan<byte> buffer, RadixTreePrefixSpan startPrefix)
			{
				_buffer = buffer;

				var rootOffset = _getNodeOffset(buffer, startPrefix.AsBytes());
				if (rootOffset < 0)
				{
					_stop = true;
				}

				else
				{
					_accumulator = new(
						new PrivateEnums.DepthFirstNodes(buffer, rootOffset, includeRoot: false)
					);
				}
			}

			public bool MoveNext()
			{
				if (_stop)
				{
					return false;
				}

				return _accumulator.TryContinueUntilNodeWithValue();
			}

			public RadixTreePrefix GetCurrentPrefix(out IValueBuffer valueBuffer)
			{
				valueBuffer = _getCurentValueBuffer();
				return GetCurrentPrefix();
			}

			public RadixTreePrefix GetCurrentPrefix()
			{
				// TODO: remove this loop and store current length in accumulator
				var prefixLength = 0;
				foreach (var info in _accumulator.EnumerateCurrentPathTopBottom())
				{
					var part = _getNodePrefix(_buffer, info);
					prefixLength += part.Length;
				}

				var i = 0;
				var prefix = new byte[prefixLength];
				var pspan = prefix.AsSpan();
				foreach (var info in _accumulator.EnumerateCurrentPathTopBottom())
				{
					var part = _getNodePrefix(_buffer, info);
					part.CopyTo(pspan[i..]);
					i += part.Length;
				}

				return new RadixTreePrefix(prefix);
			}

			private IValueBuffer _getCurentValueBuffer()
			{
				var info = _accumulator.EnumerateCurrentPathTopBottom().Last();
				Debug.Assert(info.Descriptor.HasValue);

				var vd = _getValueDescriptor(_buffer, info.Offset + PrefixDescriptor.BinaryLength);
				var vb = _getValueBuffer(_buffer, vd);
				return ValueBufferFactory.CreateFromRawBuffer(vb, vd.Marker);
			}
		}
	}
}
