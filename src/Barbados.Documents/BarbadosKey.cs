using System;

using Barbados.Documents.RadixTree;

namespace Barbados.Documents
{
	public readonly partial struct BarbadosKey
	{
		public static implicit operator BarbadosKey(string key) => new(key);

		public static bool operator ==(BarbadosKey a, BarbadosKey b) => a.SearchPrefix.AsBytes().SequenceEqual(b.SearchPrefix.AsBytes());
		public static bool operator !=(BarbadosKey a, BarbadosKey b) => !(a == b);

		public static char NestingSeparator { get; } = '.';

		private static readonly RadixTreePrefix _nestingSepAsPrefix = new(NestingSeparator.ToString());
		private static readonly string _brokenNesting = new(NestingSeparator, 2);

		public bool IsDocument { get; }

		internal RadixTreePrefix SearchPrefix { get; }

		public BarbadosKey(string key) : this(key.AsSpan())
		{

		}

		public BarbadosKey(ReadOnlySpan<char> key)
		{
			if (key.IsEmpty)
			{
				throw new ArgumentException("Key may not be empty", nameof(key));
			}

			if (key[0] == NestingSeparator)
			{
				throw new ArgumentException("Key may not start with a nesting separator", nameof(key));
			}

			if (key.Contains(_brokenNesting.AsSpan(), StringComparison.InvariantCulture))
			{
				throw new ArgumentException($"Key '{key}' contains an invalid nesting sequence", nameof(key));
			}

			IsDocument = key[^1] == NestingSeparator;
			SearchPrefix = new RadixTreePrefix(key);
		}

		internal BarbadosKey(RadixTreePrefix prefix)
		{
			IsDocument = prefix.AsBytes().EndsWith(_nestingSepAsPrefix.AsBytes());
			SearchPrefix = prefix;
		}

		internal BarbadosKey GetValueKey()
		{
			if (!IsDocument)
			{
				return this;
			}

			return new BarbadosKey(SearchPrefix.AsSpan()[..^1].ToString());
		}

		internal BarbadosKey GetDocumentKey()
		{
			if (IsDocument)
			{
				return this;
			}

			return new BarbadosKey(SearchPrefix.ToString() + NestingSeparator);
		}

		public override bool Equals(object? obj) => obj is BarbadosKey key && key == this;

		public override int GetHashCode()
		{
			var h = new HashCode();
			h.AddBytes(SearchPrefix.AsBytes());
			return h.ToHashCode();
		}

		public override string ToString() => SearchPrefix.ToString();
	}
}
