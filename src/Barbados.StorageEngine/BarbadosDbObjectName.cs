using System;

namespace Barbados.StorageEngine
{
	public readonly struct BarbadosDbObjectName
	{
		public static implicit operator BarbadosDbObjectName(string name) => new(name);
		public static implicit operator string(BarbadosDbObjectName name) => name.Name;

		public static bool operator ==(BarbadosDbObjectName a, BarbadosDbObjectName b) => a.Name == b.Name;
		public static bool operator !=(BarbadosDbObjectName a, BarbadosDbObjectName b) => a.Name != b.Name;

		public string Name { get; }

		public BarbadosDbObjectName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("Database object name may not be empty", nameof(name));
			}

			Name = name;
		}

		public bool IsReserved() =>
			Name.AsSpan().TrimStart().StartsWith(BarbadosDbObjects.ReservedNamePrefix);

		public override string ToString() => Name;

		public override bool Equals(object? obj) => obj is BarbadosDbObjectName name && name == this;

		public override int GetHashCode() => Name.GetHashCode();
	}
}
