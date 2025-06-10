using System;
using System.Collections;
using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal sealed class FindCursor : BaseCursor, ICursor<BarbadosDocument>
	{
		public static BarbadosDocument FetchDocument(FindOptions options, BTreeContext.Enumerator enumerator)
		{
			if (enumerator.TryGetCurrentDataAsSpan(out var span))
			{
				return options.InclusiveProjection switch
				{
					null => BarbadosDocument.Builder.FromBytes(span),
					true => BarbadosDocument.Builder.FromBytesInclude(span, options.Projection!),
					false => BarbadosDocument.Builder.FromBytesExclude(span, options.Projection!)
				};
			}

			else
			{
				if (!enumerator.TryGetCurrentData(out var bytes))
				{
					throw BarbadosInternalErrorExceptionHelpers.CouldNotRetrieveDataFromEnumeratorAfterMoveNext();
				}

				return options.InclusiveProjection switch
				{
					// Non-span version takes the byte array and creates a document instance with no allocations
					null => BarbadosDocument.Builder.FromBytes(bytes),
					true => BarbadosDocument.Builder.FromBytesInclude(bytes, options.Projection!),
					false => BarbadosDocument.Builder.FromBytesExclude(bytes, options.Projection!)
				};
			}
		}

		private readonly FindOptions _options;
		private readonly Lazy<BTreeContext> _context;

		public FindCursor(Lazy<TransactionScope> transaction, Lazy<BTreeContext> context, FindOptions options)
			: base(transaction)
		{
			_options = options;
			_context = context;
		}

		public IEnumerator<BarbadosDocument> GetEnumerator()
		{
			EnsureNotOpen();
			var context = _context.Value;
			var e = context.GetDataEnumerator(_options.Options);
			while (e.MoveNext())
			{
				EnsureNotClosed();
				yield return FetchDocument(_options, e);
			}

			Close();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
