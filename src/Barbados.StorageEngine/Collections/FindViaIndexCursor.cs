using System;
using System.Collections;
using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class FindViaIndexCursor : BaseCursor, ICursor<BarbadosDocument>
	{
		private readonly FindOptions _options;
		private readonly Lazy<BaseIndexContext> _indexContext;
		private readonly Lazy<BTreeContext> _collectionContext;

		public FindViaIndexCursor(Lazy<TransactionScope> transaction, Lazy<BaseIndexContext> indexContext, Lazy<BTreeContext> collectionContext, FindOptions options)
			: base(transaction)
		{
			_options = options;
			_indexContext = indexContext;
			_collectionContext = collectionContext;
		}

		public IEnumerator<BarbadosDocument> GetEnumerator()
		{
			EnsureNotOpen();
			var indexContext = _indexContext.Value;
			var collectionContext = _collectionContext.Value;
			var e = indexContext.GetEnumerator(_options.Options);
			while (e.MoveNext())
			{
				EnsureNotClosed();
				if (!e.TryGetCurrent(out BTreeNormalisedValue pkv))
				{
					if (!e.TryGetCurrentAsSpan(out var pk))
					{
						throw BarbadosInternalErrorExceptionHelpers.CouldNotRetrieveDataFromEnumeratorAfterMoveNext();
					}

					pkv = new BTreeNormalisedValue(pk.Bytes.ToArray());
				}

				var pkFindOptions = BTreeFindOptions.CreateFindSingle(pkv);
				var pkEnum = collectionContext.GetDataEnumerator(pkFindOptions);
				if (!pkEnum.MoveNext())
				{
					throw new BarbadosInternalErrorException(
						"Could not find the record in a primary key btree, even though the entry exists in the index"
					);
				}

				yield return FindCursor.FetchDocument(_options, pkEnum);
			}

			Close();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
