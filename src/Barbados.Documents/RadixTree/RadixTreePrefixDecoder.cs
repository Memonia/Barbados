using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Barbados.Documents.RadixTree
{
	internal ref struct RadixTreePrefixDecoder
	{
		[InlineArray(RadixTreeBuffer.MaxNodePrefixLength)]
		private struct PrefixBuffer
		{
			private char _f;
		}

		private readonly Decoder _decoder;
		private readonly StringBuilder _builder;

		// Each node's prefix has an upper length limit and is small enough to fit on the stack
		private PrefixBuffer _buffer;

		public RadixTreePrefixDecoder()
		{
			_decoder = Encoding.UTF8.GetDecoder();
			_builder = new();
			_buffer = new();
		}

		public readonly void Reset()
		{
			_decoder.Reset();
			_builder.Clear();
		}

		public void AppendCharsFrom(ReadOnlySpan<byte> prefix)
		{
			Debug.Assert(prefix.Length <= RadixTreeBuffer.MaxNodePrefixLength);

			Span<char> bspan = _buffer;
			var written = _decoder.GetChars(prefix, bspan, flush: false);
			_builder.Append(bspan[..written]);
		}

		public readonly string CreateStringAndReset()
		{
			var str = _builder.ToString();

			Reset();
			return str;
		}
	}
}
