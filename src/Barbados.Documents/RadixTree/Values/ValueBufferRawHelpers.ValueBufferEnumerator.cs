using System;

namespace Barbados.Documents.RadixTree.Values
{
	internal partial class ValueBufferRawHelpers
	{
		private ref struct ValueBufferEnumerator
		{
			public readonly int Count => _count;
			public readonly int CurrentIndex => _currentIndex;

			private readonly int _count;
			private readonly int _valueBufferLength;
			private readonly ReadOnlySpan<byte> _buffer;

			private int _currentIndex;
			private int _currentOffset;

			public ValueBufferEnumerator(ReadOnlySpan<byte> buffer, int valueBufferLength)
			{
				_count = GetArrayBufferCount(buffer);
				_valueBufferLength = valueBufferLength;
				_buffer = buffer;

				_currentIndex = -1;
				_currentOffset = sizeof(int);
			}

			public bool TryGetNext(out ReadOnlySpan<byte> valueBuffer)
			{
				_currentIndex += 1;
				if (_currentIndex >= _count)
				{
					valueBuffer = default;
					return false;
				}

				valueBuffer = _buffer.Slice(_currentOffset, _valueBufferLength);

				_currentOffset += _valueBufferLength;
				return true;
			}
		}
	}
}
