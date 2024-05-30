using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Indexing;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Metadata;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class BarbadosCollection : AbstractCollection
	{
		private static void _throwContainsInternalField(BarbadosDocument document)
		{
			if (document.Buffer.PrefixExists(BarbadosIdentifiers.PrefixInternal.StringBufferValue))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.InvalidDocument, "'?<name>' format is reserved for internal use"
				);
			}
		}

		public BarbadosCollection(
			BarbadosIdentifier name,
			PageHandle collectionPageHandle,
			PagePool pool,
			LockAutomatic @lock,
			BTreeClusteredIndex clusteredIndex
		) : base(name, collectionPageHandle, pool, @lock, clusteredIndex)
		{

		}

		public new ObjectId Insert(BarbadosDocument document)
		{
			_throwContainsInternalField(document);
			return base.Insert(document);
		}

		public new bool TryUpdate(ObjectId id, BarbadosDocument document)
		{
			_throwContainsInternalField(document);
			return base.TryUpdate(id, document);
		}
	}
}
