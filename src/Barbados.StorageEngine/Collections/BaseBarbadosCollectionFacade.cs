using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Barbados.Documents;
using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Collections.Extensions;
using Barbados.StorageEngine.Collections.Indexes;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Collections
{
	internal abstract class BaseBarbadosCollectionFacade
	{
		private static readonly BTreeNormalisedValue _automaticIdLookupKey = BTreeNormalisedValue.Create("?autoId", isKeyExternal: true);

		protected static BaseIndexContext GetIndexContext(IndexInfo info, TransactionScope transaction)
		{
			return new NonUniqueIndexContext(info, new BTreeContext(info.BTreeInfo, transaction));
		}

		public ObjectId Id => Info.CollectionId;
		public AutomaticIdGeneratorMode AutomaticIdGeneratorMode => Info.AutomaticIdGeneratorMode;
		public abstract BarbadosDbObjectName Name { get; }

		protected CollectionInfo Info { get; }
		protected TransactionManager TransactionManager { get; }

		private readonly Func<long, long> _automaticIdGenerator;

		public BaseBarbadosCollectionFacade(CollectionInfo info, TransactionManager transactionManager)
		{
			Info = info;
			TransactionManager = transactionManager;

			_automaticIdGenerator = info.AutomaticIdGeneratorMode switch
			{
				// Monotonically decreasing keys are faster to insert into our btree. Each time the
				// greatest key in a page changes, this needs to be propagated to the parent pages.
				// Inserting keys in increasing order makes the update propagate on every single insert.
				// On the other hand, inserting keys in decreasing order makes the update propagate only
				// on the first insert into the page or after a split 
				AutomaticIdGeneratorMode.BetterWritePerformance => (index) => long.MaxValue - index,

				// Inserting keys in some sorted order leads to the worst space utilisation, which is
				// a half of each page in our btree, since no such case handling has been implemented yet.
				// A simple function below can make a drastic difference in space utilisation by introducing
				// more "bouncing" of inserts between pages.
				// TODO: some sort of PRNG which can yield the whole range of 'long' values might result
				// in better space utilisation
				AutomaticIdGeneratorMode.BetterSpaceUtilisation => (index) => index ^ 9223372036854775783,
				_ => throw new NotImplementedException(),
			};
		}

		public void Deallocate()
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			foreach (var indexInfo in EnumerateIndexes())
			{
				var indexContext = GetIndexContext(indexInfo, tx);
				indexContext.Deallocate();
			}

			_getBTreeContext(tx).Deallocate();
			TransactionManager.CommitTransaction(tx);
		}

		public void IndexBuild(IndexInfo info)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var indexContext = GetIndexContext(info, tx);
			using var cursor = Find(FindOptions.All);
			foreach (var document in cursor)
			{
				if (!document.TryGetNormalised(BarbadosDocumentKeys.DocumentId, out var pk))
				{
					throw new BarbadosInternalErrorException(
						"Found a document without a primary key. Existing documents must have a primary key"
					);
				}

				if (document.TryGetNormalised(info.Field, out var sk))
				{
					if (!indexContext.TryInsert(pk, sk))
					{
						throw new BarbadosInternalErrorException(
							"Tried to insert a duplicate entry. Method must be called on an empty index"
						);
					}
				}
			}

			TransactionManager.CommitTransaction(tx);
		}

		public void IndexDeallocate(IndexInfo info)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var indexContext = GetIndexContext(info, tx);
			indexContext.Deallocate();
			TransactionManager.CommitTransaction(tx);
		}

		public BarbadosDocument InsertWithAutomaticId(BarbadosDocument.Builder builder)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var context = _getBTreeContext(tx);

			var nextId = _getNextAutomaticObjectId(context);
			var document = builder
				.Add(BarbadosDocumentKeys.DocumentId, nextId.Value)
				.Build();

			Insert(document);
			TransactionManager.CommitTransaction(tx);
			return document;
		}

		public void Insert(BarbadosDocument document)
		{
			if (!TryInsert(document))
			{
				throw new BarbadosException(
					BarbadosExceptionCode.DocumentAlreadyExists, "Document with a given primary key already exists"
				);
			}
		}

		public void Update(BarbadosDocument document)
		{
			if (!TryUpdate(document))
			{
				BarbadosCollectionExceptionHelpers.ThrowDocumentWithPrimaryKeyWasNotFound();
			}
		}

		public void Remove(BarbadosDocument document)
		{
			if (!TryRemove(document))
			{
				BarbadosCollectionExceptionHelpers.ThrowDocumentWithPrimaryKeyWasNotFound();
			}
		}

		public bool TryInsert(BarbadosDocument document)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var context = _getBTreeContext(tx);
			if (_tryInsert(context, document))
			{
				TransactionManager.CommitTransaction(tx);
				return true;
			}

			return false;
		}

		public bool TryUpdate(BarbadosDocument document)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var context = _getBTreeContext(tx);
			if (_tryRemove(context, document) && _tryInsert(context, document))
			{
				TransactionManager.CommitTransaction(tx);
				return true;
			}

			return false;
		}

		public bool TryRemove(BarbadosDocument document)
		{
			using var tx = TransactionManager.GetAutomaticTransaction(Id, TransactionMode.ReadWrite);
			var context = _getBTreeContext(tx);
			if (_tryRemove(context, document))
			{
				TransactionManager.CommitTransaction(tx);
				return true;
			}

			return false;
		}

		public ICursor<BarbadosDocument> Find(FindOptions options)
		{
			var ltx = new Lazy<TransactionScope>(() => TransactionManager.GetAutomaticTransaction(Id, TransactionMode.Read));
			var lctx = new Lazy<BTreeContext>(() => _getBTreeContext(ltx.Value));
			return new FindCursor(ltx, lctx, options);
		}

		public ICursor<BarbadosDocument> Find(FindOptions options, BarbadosKey indexField)
		{
			var ltx = new Lazy<TransactionScope>(() => TransactionManager.GetAutomaticTransaction(Id, TransactionMode.Read));
			var lcctx = new Lazy<BTreeContext>(() => _getBTreeContext(ltx.Value));
			var lictx = new Lazy<BaseIndexContext>(() => GetIndexContext(GetIndexInfo(indexField), ltx.Value));
			return new FindViaIndexCursor(ltx, lictx, lcctx, options);
		}

		protected abstract IndexInfo GetIndexInfo(BarbadosKey field);
		protected abstract IEnumerable<IndexInfo> EnumerateIndexes();

		private ObjectId _getNextAutomaticObjectId(BTreeContext context)
		{
			long index = 1;
			if (context.TryFind(_automaticIdLookupKey, out var indexBytes))
			{
				index = HelpRead.AsInt64(indexBytes);
			}

			if (index == long.MaxValue)
			{
				throw new BarbadosException(
					BarbadosExceptionCode.MaxAutomaticIdCountReached, "Automatic id sequence has been exhausted"
				);
			}

			Span<byte> nextIdBytes = stackalloc byte[sizeof(long)];
			HelpWrite.AsInt64(nextIdBytes, index + 1);

			context.TryRemove(_automaticIdLookupKey);
			var r = context.TryInsert(_automaticIdLookupKey, nextIdBytes);
			Debug.Assert(r);
			return new(_automaticIdGenerator(index));
		}

		private bool _tryInsert(BTreeContext context, BarbadosDocument document)
		{
			if (!document.TryGetNormalised(BarbadosDocumentKeys.DocumentId, out var pk))
			{
				throw BarbadosArgumentExceptionHelpers.NoPrimaryKeyField();
			}

			if (context.TryInsert(pk, document.AsBytes()))
			{
				foreach (var indexInfo in EnumerateIndexes())
				{
					if (document.TryGetNormalised(indexInfo.Field, out var sk))
					{
						var ictx = GetIndexContext(indexInfo, context.Transaction);
						if (!ictx.TryInsert(pk, sk))
						{
							throw new BarbadosInternalErrorException(
								"Index contains a duplicate entry, even though collection insert succeeded"
							);
						}
					}
				}

				return true;
			}

			return false;
		}

		private bool _tryRemove(BTreeContext context, BarbadosDocument document)
		{
			if (!document.TryGetNormalised(BarbadosDocumentKeys.DocumentId, out var pk))
			{
				throw BarbadosArgumentExceptionHelpers.NoPrimaryKeyField();
			}

			using (var cursor = Find(FindOptions.Single(pk)))
			{
				// Input only needs to contain the primary key. We fetch the full document in order to update indexes
				document = cursor.FirstOrDefault()!;
			}

			if (document is null)
			{
				return false;
			}

			if (!context.TryRemove(pk))
			{
				throw new BarbadosInternalErrorException("Could not remove a document which was previously found");
			}

			foreach (var indexInfo in EnumerateIndexes())
			{
				if (document.TryGetNormalised(indexInfo.Field, out var sk))
				{
					var ictx = GetIndexContext(indexInfo, context.Transaction);
					if (!ictx.TryRemove(pk, sk))
					{
						throw new BarbadosInternalErrorException(
							"Could not remove an index entry, even though the document was successfully removed from the collection"
						);
					}
				}
			}

			return true;
		}

		private BTreeContext _getBTreeContext(TransactionScope transaction)
		{
			return new BTreeContext(Info.BTreeInfo, transaction);
		}
	}
}
