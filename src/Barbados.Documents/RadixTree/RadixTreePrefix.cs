using System;
using System.Text;

namespace Barbados.Documents.RadixTree
{
	internal sealed class RadixTreePrefix
	{

		public static implicit operator RadixTreePrefixSpan(RadixTreePrefix prefix) => prefix.AsSpan();

		public static int GetLength(ReadOnlySpan<char> prefix) => Encoding.UTF8.GetByteCount(prefix);
		public static int GetLength(ReadOnlySpan<char> prefix, ReadOnlySpan<char> append) => Encoding.UTF8.GetByteCount(prefix) + Encoding.UTF8.GetByteCount(append);
		public static void Write(ReadOnlySpan<char> prefix, in Span<byte> destination) => Encoding.UTF8.GetBytes(prefix, destination);
		public static void Write(ReadOnlySpan<char> prefix, ReadOnlySpan<char> append, in Span<byte> destination)
		{
			Encoding.UTF8.GetBytes(prefix, destination);
			Encoding.UTF8.GetBytes(append, destination[prefix.Length..]);
		}

		public static RadixTreePrefix Empty { get; } = new(string.Empty);

		public int Length => _prefix.Length;

		private readonly byte[] _prefix;

		public RadixTreePrefix(byte[] prefix)
		{
			_prefix = prefix;
		}

		public RadixTreePrefix(ReadOnlySpan<char> prefix)
		{
			_prefix = new byte[GetLength(prefix)];
			Write(prefix, _prefix);
		}

		public RadixTreePrefix(RadixTreePrefixSpan prefix) : this(prefix.AsBytes().ToArray())
		{

		}

		public RadixTreePrefixSpan AsSpan() => new(_prefix);
		public ReadOnlySpan<byte> AsBytes() => AsSpan().AsBytes();

		public override string ToString() => AsSpan().ToString();
		public string ToString(int truncateLength) => AsSpan().ToString(truncateLength);

		public RadixTreePrefixSpan this[Range range] => AsSpan()[range];
	}
}
