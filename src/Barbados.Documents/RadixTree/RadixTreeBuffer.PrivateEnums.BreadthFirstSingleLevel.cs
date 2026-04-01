using System;

namespace Barbados.Documents.RadixTree
{
	internal partial class RadixTreeBuffer
	{
		private sealed partial class PrivateEnums
		{
			public ref struct BreadthFirstSingleLevel
			{
				private readonly ReadOnlySpan<byte> _buffer;
				private int _currentOffset;

				public BreadthFirstSingleLevel(ReadOnlySpan<byte> buffer, int firstNodeOffset)
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
}
