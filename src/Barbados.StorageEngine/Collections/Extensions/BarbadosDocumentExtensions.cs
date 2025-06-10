using Barbados.Documents;
using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Collections.Extensions
{
	internal static class BarbadosDocumentExtensions
	{
		public static ObjectId GetObjectId(this BarbadosDocument document)
		{
			return new(document.GetInt64(BarbadosDocumentKeys.DocumentId));
		}

		public static bool TryGetNormalised(this BarbadosDocument document, BarbadosKey key, out BTreeNormalisedValue value)
		{
			if (document.TryGet(key, out var v))
			{
				value = BTreeNormalisedValue.Create(v, isKeyExternal: false);
				return true;
			}

			value = default!;
			return false;
		}
	}
}
