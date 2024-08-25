using System;
using System.Collections.Generic;

using Barbados.StorageEngine.Documents.Binary;

namespace Barbados.StorageEngine
{
	public sealed class BarbadosIdentifier
	{
		public static implicit operator BarbadosIdentifier(string identifier) => new(identifier);
		public static implicit operator string(BarbadosIdentifier identifier) => identifier.Identifier;

		public static bool operator ==(BarbadosIdentifier a, BarbadosIdentifier b) => a.Identifier == b.Identifier;
		public static bool operator !=(BarbadosIdentifier a, BarbadosIdentifier b) => a.Identifier != b.Identifier;

		public bool IsGroup { get; }
		public bool IsReserved { get; }
		public string Identifier { get; }

		internal ValueName BinaryName { get; }

		public BarbadosIdentifier(string identifier)
		{
			if (string.IsNullOrEmpty(identifier))
			{
				throw new ArgumentException("Identifier may not be empty", nameof(identifier));
			}

			if (identifier.StartsWith(CommonIdentifiers.NestingSeparator))
			{
				throw new ArgumentException("Identifier may not start with a nesting separator", nameof(identifier));
			}

			if (identifier.TrimStart().StartsWith(CommonIdentifiers.ReservedNamePrefix))
			{
				IsReserved = true;
			}

			var consecutiveNesting = 0;
			foreach (var c in identifier)
			{
				consecutiveNesting += 1;
				if (c != CommonIdentifiers.NestingSeparator[0])
				{
					consecutiveNesting = 0;
				}

				if (consecutiveNesting > 1)
				{
					throw new ArgumentException($"Broken nesting in '{identifier}'", nameof(identifier));
				}
			}

			IsGroup = identifier.EndsWith(CommonIdentifiers.NestingSeparator[0]);
			Identifier = identifier;
			BinaryName = new(identifier);
		}

		public override string ToString() => Identifier;

		internal string GetGroupName()
		{
			if (!IsGroup)
			{
				throw new InvalidOperationException("This instance is not a group identifier");
			}

			return Identifier[..^1];
		}

		internal string GetGroupIdentifier()
		{
			if (IsGroup)
			{
				throw new InvalidOperationException("This instance is not a value identifier");
			}

			return Identifier + CommonIdentifiers.NestingSeparator[0];
		}

		internal IEnumerator<int> GetSplitIndices()
		{
			var i = 0;
			while (i < Identifier.Length)
			{
				var j = Identifier.IndexOf(CommonIdentifiers.NestingSeparator[0], i);
				if (j == -1)
				{
					j = Identifier.Length;
				}

				yield return j;
				i = j + 1;
			}
		}

		public override bool Equals(object? obj)
		{
			if (obj is BarbadosIdentifier identifier)
			{
				return Identifier == identifier.Identifier;
			}

			return false;
		}

		public override int GetHashCode() => Identifier.GetHashCode();
	}
}
