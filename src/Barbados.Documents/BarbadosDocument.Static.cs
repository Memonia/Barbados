using Barbados.Documents.Serialisation.Values;

namespace Barbados.Documents
{
	public partial class BarbadosDocument
	{
		public static bool TryCompareFields(BarbadosKey field, BarbadosDocument a, BarbadosDocument b, out int result)
		{
			if (a._buffer.TryGetBufferRaw(field.SearchPrefix.AsBytes(), out var ma, out var va) &&
				b._buffer.TryGetBufferRaw(field.SearchPrefix.AsBytes(), out var mb, out var vb) &&
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
	}
}
