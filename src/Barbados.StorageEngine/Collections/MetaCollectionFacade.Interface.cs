using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;

namespace Barbados.StorageEngine.Collections
{
	internal partial class MetaCollectionFacade
	{
		BarbadosIdentifier IReadOnlyBarbadosCollection.Name => CommonIdentifiers.Collections.MetaCollection;

		bool IReadOnlyBarbadosCollection.TryGetBTreeIndex(string field, out IReadOnlyBTreeIndex index)
		{
			if (field == CommonIdentifiers.MetaCollection.AbsCollectionDocumentNameField.Identifier)
			{
				index = _nameIndexFacade;
				return true;
			}

			index = default!;
			return false;
		}

		bool IReadOnlyBarbadosCollection.TryRead(ObjectId id, out BarbadosDocument document)
		{
			return TryRead(id, ValueSelector.SelectAll, out document);
		}

		bool IReadOnlyBarbadosCollection.TryRead(ObjectId id, ValueSelector selector, out BarbadosDocument document)
		{
			return TryRead(id, selector, out document);
		}

		BarbadosDocument IReadOnlyBarbadosCollection.Read(ObjectId id)
		{
			return ((IReadOnlyBarbadosCollection)this).Read(id, ValueSelector.SelectAll);
		}

		BarbadosDocument IReadOnlyBarbadosCollection.Read(ObjectId id, ValueSelector selector)
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

		ICursor<BarbadosDocument> IReadOnlyBarbadosCollection.GetCursor(ValueSelector selector)
		{
			return GetCursor(selector);
		}
	}
}
