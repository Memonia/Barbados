using System;
using System.Diagnostics;

using Barbados.Documents.RadixTree;

namespace Barbados.Documents
{
	public readonly partial struct BarbadosKey
	{
		public static implicit operator BarbadosKey(string key) => new(key);

		public static bool operator ==(BarbadosKey a, BarbadosKey b) => a.Equals(b);
		public static bool operator !=(BarbadosKey a, BarbadosKey b) => !a.Equals(b);

		public static string NestingSeparator { get; } = ".";

		internal static RadixTreePrefix NestingSeparatorAsPrefix { get; } = new(NestingSeparator);

		private static readonly string _brokenNesting = NestingSeparator + NestingSeparator;

		// All keys are stored as document prefixes. This way we never need to allocate anything when manipulating keys:
		// if we need to retrieve a document with given prefix, we return stored prefix as is; if we need to retrieve a
		// value, we return a read-only view of the prefix up until the last nesting separator
		internal RadixTreePrefixSpan DocumentSearchPrefix => _documentPrefix;
		internal RadixTreePrefixSpan ValueSearchPrefix => _documentPrefix[..^NestingSeparatorAsPrefix.Length];

		private readonly RadixTreePrefix _documentPrefix;

		public BarbadosKey(ReadOnlySpan<char> key)
		{
			if (key.IsEmpty)
			{
				throw new ArgumentException("The key may not be empty", nameof(key));
			}

			if (key[..NestingSeparator.Length].SequenceEqual(NestingSeparator))
			{
				throw new ArgumentException("The key may not start with a nesting separator", nameof(key));
			}

			if (key[^NestingSeparator.Length..].SequenceEqual(NestingSeparator))
			{
				throw new ArgumentException("The key may not end with a nesting separator", nameof(key));
			}

			if (key.Contains(_brokenNesting.AsSpan(), StringComparison.InvariantCulture))
			{
				throw new ArgumentException($"The key contains an invalid nesting sequence", nameof(key));
			}

			var b = new byte[RadixTreePrefix.GetLength(key) + RadixTreePrefix.GetLength(NestingSeparator)];
			RadixTreePrefix.Write(key, NestingSeparator, b);

			_documentPrefix = new RadixTreePrefix(b);
		}

		internal BarbadosKey(RadixTreePrefix prefix)
		{
			// uhh...
			Debug.Assert((
				(Func<bool>)(() => {
					var MUST_NOT_THROW = new BarbadosKey(prefix.ToString());
					return true;
				})
			)());

			if (prefix[^NestingSeparator.Length..].SequenceEqual(NestingSeparatorAsPrefix))
			{
				_documentPrefix = prefix;
			}

			else
			{
				var b = new byte[prefix.Length + RadixTreePrefix.GetLength(NestingSeparator)];
				prefix.AsSpan().AsBytes().CopyTo(b);
				RadixTreePrefix.Write(NestingSeparator, b.AsSpan()[prefix.Length..]);

				_documentPrefix = new RadixTreePrefix(b);
			}
		}

		public override bool Equals(object? obj)
			=> obj is BarbadosKey key && key.ValueSearchPrefix.AsBytes().SequenceEqual(this.ValueSearchPrefix.AsBytes());

		public override int GetHashCode()
		{
			var h = new HashCode();
			h.AddBytes(ValueSearchPrefix.AsBytes());
			return h.ToHashCode();
		}

		public override string ToString() => ValueSearchPrefix.ToString();
	}
}
