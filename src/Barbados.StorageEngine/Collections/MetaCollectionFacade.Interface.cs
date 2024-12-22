using Barbados.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Collections
{
	internal partial class MetaCollectionFacade
	{
		BarbadosDbObjectName IReadOnlyBarbadosCollection.Name => BarbadosDbObjects.Collections.MetaCollection;

		bool IReadOnlyBarbadosCollection.TryGetBTreeIndex(string field, out IReadOnlyBTreeIndex index)
		{
			if (field == BarbadosDocumentKeys.MetaCollection.AbsCollectionDocumentNameField)
			{
				index = _nameIndexFacade;
				return true;
			}

			index = default!;
			return false;
		}

		bool IReadOnlyBarbadosCollection.TryRead(ObjectId id, out BarbadosDocument document)
		{
			return TryRead(id, BarbadosKeySelector.SelectAll, out document);
		}

		bool IReadOnlyBarbadosCollection.TryRead(ObjectId id, BarbadosKeySelector selector, out BarbadosDocument document)
		{
			return TryRead(id, selector, out document);
		}

		BarbadosDocument IReadOnlyBarbadosCollection.Read(ObjectId id)
		{
			return ((IReadOnlyBarbadosCollection)this).Read(id, BarbadosKeySelector.SelectAll);
		}

		BarbadosDocument IReadOnlyBarbadosCollection.Read(ObjectId id, BarbadosKeySelector selector)
		{
			if (!TryRead(id, selector, out var document))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DocumentNotFound, $"Document with id {id} not found"
				);
			}

			return document;
		}

		ICursor<BarbadosDocument> IReadOnlyBarbadosCollection.GetCursor()
		{
			return GetCursor();
		}

		ICursor<BarbadosDocument> IReadOnlyBarbadosCollection.GetCursor(BarbadosKeySelector selector)
		{
			return GetCursor(selector);
		}
	}
}
