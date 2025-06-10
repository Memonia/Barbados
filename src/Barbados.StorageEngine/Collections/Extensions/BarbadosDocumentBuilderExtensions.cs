using Barbados.Documents;

namespace Barbados.StorageEngine.Collections.Extensions
{
	internal static class BarbadosDocumentBuilderExtensions
	{
		public static BarbadosDocument.Builder AddObjectId(this BarbadosDocument.Builder builder, ObjectId id)
		{
			return builder.Add(BarbadosDocumentKeys.DocumentId, id.Value);
		}
	}
}
