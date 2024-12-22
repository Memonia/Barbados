using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Helpers;
using Barbados.StorageEngine.Indexing.Extensions;
using Barbados.StorageEngine.Indexing.Search;
using Barbados.StorageEngine.Storage.Paging.Pages;
using Barbados.StorageEngine.Transactions;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed class BTreeIndexFindCursor : Cursor<ObjectId>
	{
		private readonly BTreeIndexFacade _indexFacade;
		private readonly BTreeClusteredIndexFacade _clusteredIndexfacade;
		private readonly BTreeIndexFindCursorParameters _params;
		public BTreeIndexFindCursor(
			ObjectId collectionId,
			TransactionManager transactionManager,
			BTreeIndexFacade indexFacade,
			BTreeClusteredIndexFacade clusteredIndexfacade,
			BTreeIndexFindCursorParameters parameters
		) : base(collectionId, transactionManager)
		{
			_indexFacade = indexFacade;
			_clusteredIndexfacade = clusteredIndexfacade;
			_params = parameters;
		}

		protected override IEnumerable<ObjectId> EnumerateValues(TransactionScope transaction)
		{
			BTreeIndexKey search;
			if (_params.Ascending)
			{
				search = _indexFacade.ToBTreeIndexKey(_params.Check.LowerBound);
			}

			else
			{
				search = _indexFacade.ToBTreeIndexKey(_params.Check.UpperBound);
			}

			var proxy = _indexFacade.GetProxy(transaction);
			if (!proxy.TryFind(search, out var traceback))
			{
				yield break;
			}

			var ids = new List<ObjectId>();
			var start = traceback.Current;
			var take = _params.Take;
			var cproxy = _clusteredIndexfacade.GetProxy(transaction);
			foreach (
				var leaf in _params.Ascending
				? ChainHelpers.EnumerateForwardsPinned<BTreeLeafPage>(transaction, start)
				: ChainHelpers.EnumerateBackwardsPinned<BTreeLeafPage>(transaction, start)
			)
			{
				_retrieveIds(cproxy, in ids, leaf, _params.Check);
				if (ids.Count > 0)
				{
					foreach (var id in ids)
					{
						if (_params.TakeAll)
						{
							yield return id;
						}

						else
						{
							if (take <= 0)
							{
								yield break;
							}

							take -= 1;
							yield return id;
						}
					}

					ids.Clear();
				}

				else
				{
					yield break;
				}
			}
		}

		private bool _tryGetOriginalKey(
			BTreeClusteredIndexTransactionProxy proxy,
			ObjectId id,
			out NormalisedValue key
		)
		{
			var idn = new ObjectIdNormalised(id);
			if (!proxy.TryReadDocument(idn, _indexFacade.KeySelector, out var buffer))
			{
				key = default!;
				return false;
			}
			
			return buffer.TryGetNormalisedValue(_indexFacade.Info.IndexField, out key);
		}

		private void _retrieveIds(
			BTreeClusteredIndexTransactionProxy proxy, 
			in List<ObjectId> ids,
			BTreeLeafPage from,
			KeyCheckRange check
		)
		{
			bool _check(BTreeIndexKey key, ObjectId id, bool isTrimmed)
			{
				var result = false;
				if (
					!NormalisedValue.AreSameValueTypeOrInvalid(check.LowerBound.AsSpan(), key.Separator) ||
					!NormalisedValue.AreSameValueTypeOrInvalid(check.UpperBound.AsSpan(), key.Separator)
				)
				{
					result = false;
				}

				else
				if (
					isTrimmed && (
						check.LowerBound.AsSpan().Bytes.Length > _indexFacade.Info.KeyMaxLength ||
						check.UpperBound.AsSpan().Bytes.Length > _indexFacade.Info.KeyMaxLength
					)
				)
				{
					if (_tryGetOriginalKey(proxy, id, out var full))
					{
						result = check.Check(full.AsSpan());
					}
				}

				else
				{
					result =
						check.LowerBound.AsSpan().Bytes.Length <= _indexFacade.Info.KeyMaxLength &&
						check.UpperBound.AsSpan().Bytes.Length <= _indexFacade.Info.KeyMaxLength &&
						!isTrimmed && check.Check(key.Separator);
				}

				return result;
			}

			Debug.Assert(ids.Count == 0);
			var e = from.GetEnumerator();
			while (e.TryGetNext(out var storedKey))
			{
				if (from.TryReadObjectId(storedKey.Separator, out var storedId, out var isTrimmed))
				{
					Debug.Assert(isTrimmed == storedKey.IsTrimmed);
					if (_check(storedKey, storedId, isTrimmed))
					{
						ids.Add(storedId);
					}
				}

				else
				if (from.TryReadOverflowHandle(storedKey.Separator, out var start))
				{
					foreach (
					var overflow in
						ChainHelpers.EnumerateForwardsPinned<BTreeLeafPageOverflow>(proxy.Transaction, start)
					)
					{
						var oe = overflow.GetEnumerator();
						while (oe.TryGetNext(out storedId, out isTrimmed))
						{
							if (_check(storedKey, storedId, isTrimmed))
							{
								ids.Add(storedId);
							}
						}
					}
				}
			}
		}
	}
}
