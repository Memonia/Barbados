using System;
using System.Text;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal readonly ref struct RadixTreePrefixSpan
	{
		public int Length => _prefix.Length;

		private readonly ReadOnlySpan<byte> _prefix;

		public RadixTreePrefixSpan(ReadOnlySpan<byte> prefix)
		{
			_prefix = prefix;
		}

		public bool StartsWith(RadixTreePrefix other) => StartsWith(other.AsSpan());
		public bool StartsWith(RadixTreePrefixSpan other) => _prefix.StartsWith(other._prefix);
		public int CommonPrefixLength(RadixTreePrefix other) => CommonPrefixLength(other.AsSpan());
		public int CommonPrefixLength(RadixTreePrefixSpan other) => _prefix.CommonPrefixLength(other._prefix);

		public ReadOnlySpan<byte> AsBytes() => _prefix;
		public override string ToString() => Encoding.UTF8.GetString(_prefix);

		public void WriteTo(Span<byte> destination) => _prefix.CopyTo(destination);

		public RadixTreePrefixSpan this[Range range] => new(_prefix[range]);
	}
}
