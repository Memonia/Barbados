using System;
using System.Text;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal sealed class RadixTreePrefixDecoder
	{
		private readonly Decoder _decoder;
		private char[] _buffer;
		private int _currentOffset;

		public RadixTreePrefixDecoder()
		{
			_decoder = Encoding.UTF8.GetDecoder();
			_buffer = [];
			_currentOffset = 0;
		}

		public int GetRunningCharCount(ReadOnlySpan<byte> prefix)
		{
			return _decoder.GetCharCount(prefix, flush: false);
		}

		public void CreateCharBuffer(int charCount)
		{
			_buffer = new char[charCount];
			_currentOffset = 0;
		}

		public void AppendCharsFrom(ReadOnlySpan<byte> prefix)
		{
			var bspan = _buffer.AsSpan();
			_currentOffset += _decoder.GetChars(prefix, bspan[_currentOffset..], flush: false);
		}

		public char[] GetCharArrayAndReturn()
		{
			var arr = _buffer;
			Reset();

			// Unfortunately, there is no way to create a 'String' instance without extra allocations.
			// In order to minimise the damage, this method returns a char array, so that the caller
			// doesn't incur the penalty when it doesn't need a string instance.
			//
			// Everybody else will have to live with double the memory usage for no reason
			return arr;
		}

		public void Reset()
		{
			_decoder.Reset();
			_buffer = [];
			_currentOffset = 0;
		}
	}
}
