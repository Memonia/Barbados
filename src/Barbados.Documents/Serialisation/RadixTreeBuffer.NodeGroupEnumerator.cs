using System;

namespace Barbados.Documents.Serialisation
{
	internal partial class RadixTreeBuffer
	{
		private ref struct NodeGroupEnumerator
		{
			private readonly ReadOnlySpan<byte> _buffer;
			private int _currentOffset;

			public NodeGroupEnumerator(ReadOnlySpan<byte> buffer, int firstNodeOffset)
			{
				_buffer = buffer;
				_currentOffset = firstNodeOffset;
			}

			public bool TryGetNext(out NodeInfo info)
			{
				if (_currentOffset < 0)
				{
					info = default!;
					return false;
				}

				var pd = _getPrefixDescriptor(_buffer, _currentOffset);
				info = new(_currentOffset, pd);

				if (pd.IsLastChild)
				{
					_currentOffset = -1;
				}

				else
				{
					_currentOffset = _getNextNodeOffset(_buffer, new(_currentOffset, pd));
				}

				return true;
			}
		}
	}
}
