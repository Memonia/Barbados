using System;
using System.Text;

namespace Barbados.StorageEngine.Documents.Serialisation
{
	internal sealed class RadixTreePrefix
	{
		public static RadixTreePrefix Empty { get; } = new(string.Empty);

		public int Length => _prefix.Length;

		private readonly byte[] _prefix;

		public RadixTreePrefix(byte[] prefix)
		{
			_prefix = prefix;
		}

		public RadixTreePrefix(string prefix) : this(Encoding.UTF8.GetBytes(prefix))
		{

		}

		public RadixTreePrefix(RadixTreePrefixSpan prefix) : this(prefix.AsBytes().ToArray())
		{

		}

		public RadixTreePrefixSpan AsSpan() => new(_prefix);
		public ReadOnlySpan<byte> AsBytes() => AsSpan().AsBytes();

		public override string ToString() => AsSpan().ToString();

		public RadixTreePrefix this[Range range] => new(_prefix[range]);
	}
}
