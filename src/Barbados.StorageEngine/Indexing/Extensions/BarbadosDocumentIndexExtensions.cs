using Barbados.Documents;

namespace Barbados.StorageEngine.Indexing.Extensions
{
	internal static class BarbadosDocumentIndexExtensions
	{
		public static bool TryGetNormalisedValue(this BarbadosDocument document, BarbadosKey key, out NormalisedValue value)
		{
			if (document.TryGet(key, out var v))
			{
				value = NormalisedValue.Create(v);
				return true;
			}

			value = default!;
			return false;
		}
	}
}
