using System;

using Barbados.Documents.RadixTree.Values;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public static bool operator ==(BarbadosDocument a, BarbadosDocument b) => a.Equals(b);
		public static bool operator !=(BarbadosDocument a, BarbadosDocument b) => !a.Equals(b);

		public static bool TryCompare(BarbadosKey key, BarbadosDocument a, BarbadosDocument b, out int result)
		{
			// Nested documents cannot be compared
			if (a._buffer.TryGetBufferRaw(key.ValueSearchPrefix, out var ma, out var va) &&
				b._buffer.TryGetBufferRaw(key.ValueSearchPrefix, out var mb, out var vb) &&
				ma == mb
			)
			{
				var comparer = ValueBufferSpanComparerFactory.GetComparer(ma);
				result = comparer.Compare(va, vb);
				return true;
			}

			result = 0;
			return false;
		}

		public bool Equals(BarbadosDocument? other)
		{
			if (other is null)
			{
				return false;
			}

			var e = this.GetKeyEnumerator(flat: true);
			while (e.MoveNext())
			{
				var current = e.GetCurrent();
				if (TryCompare(current, this, other, out var result) && result != 0)
				{
					return false;
				}
			}

			return true;
		}

		public override int GetHashCode()
		{
			var h = new HashCode();
			h.AddBytes(this.AsBytes());
			return h.ToHashCode();
		}

		public override bool Equals(object? obj)
		{
			return obj is BarbadosDocument bbd && this.Equals(bbd);
		}
	}
}
