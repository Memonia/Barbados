using System;
using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Collections;
using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Indexing.Search;
using Barbados.StorageEngine.Indexing.Search.Checks;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed partial class BTreeIndex : AbstractBTreeIndex<BTreeLeafPage>, IReadOnlyBTreeIndex
	{
		/* Write locks are handled by the collection. Read locks are taken by cursors the index returns 
		 */

		public BarbadosIdentifier IndexedField => Info.IndexedField;

		public LockAutomatic CollectionLock { get; }
		public BTreeClusteredIndex ClusteredIndex { get; }

		public BTreeIndex(
			BTreeIndexInfo info,
			LockAutomatic collectionLock,
			BTreeClusteredIndex clusteredIndex,
			PagePool pool
		) : base(pool, info)
		{
			CollectionLock = collectionLock;
			ClusteredIndex = clusteredIndex;
		}

		public void DeallocateNoLock()
		{
			Deallocate();
		}

		public ICursor<ObjectId> Find(BarbadosDocument condition)
		{
			static void _throwInvalidCondition(bool notFalse)
			{
				if (notFalse)
				{
					throw new ArgumentException(
						"Expected a single condition with an optional '?inclusive' for a non-range condition",
						nameof(condition)
					);
				}
			}

			static NormalisedValue _retrieveSearchKey(BarbadosDocument condition)
			{
				if (
					!condition.Buffer.TryGetNormalisedValue(
						BarbadosIdentifiers.Index.SearchValue.StringBufferValue, out var value
					)
				)
				{
					throw new ArgumentException(
						$"Expected a {BarbadosIdentifiers.Index.SearchValue} for a non-range condition",
						nameof(condition)
					);
				}

				return value;
			}

			var doExact = condition.Buffer.TryGetBoolean(BarbadosIdentifiers.Index.Exact.StringBufferValue, out var ex) && ex;
			var doRange = condition.Buffer.TryGetBoolean(BarbadosIdentifiers.Index.Range.StringBufferValue, out var rg) && rg;
			var doLess = condition.Buffer.TryGetBoolean(BarbadosIdentifiers.Index.LessThan.StringBufferValue, out var lt) && lt;
			var doGreater = condition.Buffer.TryGetBoolean(BarbadosIdentifiers.Index.GreaterThan.StringBufferValue, out var gt) && gt;
			var doInclusive = condition.Buffer.TryGetBoolean(BarbadosIdentifiers.Index.Inclusive.StringBufferValue, out var incl) && incl;
			var doAsc = condition.Buffer.TryGetBoolean(BarbadosIdentifiers.Index.Ascending.StringBufferValue, out var asc) && asc;
			var doTake = condition.Buffer.TryGetInt64(BarbadosIdentifiers.Index.Take.StringBufferValue, out var take);

			if (doTake && take < 0)
			{
				throw new ArgumentException("'Take' argument must be a positive integer", nameof(condition));
			}

			KeyCheckRange check;
			IEnumerable<ObjectId> ids;
			if (doRange)
			{
				if (
					!condition.Buffer.TryGetNormalisedValue(
						BarbadosIdentifiers.Index.LessThan.StringBufferValue, out var end
					) ||
					!condition.Buffer.TryGetNormalisedValue(
						BarbadosIdentifiers.Index.GreaterThan.StringBufferValue, out var start
					)
				)
				{
					throw new ArgumentException(
						$"Could not find range bounds in '{BarbadosIdentifiers.Index.LessThan}' " +
						$"and '{BarbadosIdentifiers.Index.GreaterThan}' fields",
						nameof(condition)
					);
				}

				if (doInclusive)
				{
					check = new KeyCheckBetweenInclusive(start, end);
					ids = _read(check, take, doAsc, !doTake);

				}

				else
				{
					check = new KeyCheckBetween(start, end);
					ids = _read(check, take, doAsc, !doTake);
				}
			}

			else
			if (doExact || !(doLess || doGreater || doRange))
			{
				_throwInvalidCondition(doLess || doGreater || doRange);
				var search = _retrieveSearchKey(condition);
				check = new KeyCheckBetweenInclusive(search, search);
				ids = _read(check, take, doAsc, !doTake);
			}

			else
			if (doLess)
			{
				_throwInvalidCondition(doExact || doGreater || doRange);
				var search = _retrieveSearchKey(condition);
				if (doInclusive)
				{
					check = new KeyCheckBetweenInclusive(NormalisedValue.Min, search);
					ids = _read(check, take, doAsc, !doTake);

				}

				else
				{
					check = new KeyCheckBetween(NormalisedValue.Min, search);
					ids = _read(check, take, doAsc, !doTake);
				}
			}

			else
			if (doGreater)
			{
				_throwInvalidCondition(doExact || doLess || doRange);
				var search = _retrieveSearchKey(condition);
				if (doInclusive)
				{
					check = new KeyCheckBetweenInclusive(search, NormalisedValue.Max);
					ids = _read(check, take, doAsc, !doTake);
				}

				else
				{
					check = new KeyCheckBetween(search, NormalisedValue.Max);
					ids = _read(check, take, doAsc, !doTake);
				}
			}

			else
			{
				throw new ArgumentException("Unexpected condition", nameof(condition));
			}

			return new Cursor<ObjectId>(ids, CollectionLock);
		}

		public ICursor<ObjectId> FindExact<T>(T searchValue)
		{
			var search = searchValue switch
			{
				NormalisedValue nv => nv,
				_ => NormalisedValue.Create(searchValue)
			};

			var ids = new List<ObjectId>();
			var ikey = ToBTreeIndexKey(search);
			if (TryFind(ikey, out var traceback))
			{
				var leaf = Pool.LoadPin<BTreeLeafPage>(traceback.Current);
				var check = new KeyCheckBetweenInclusive(search, search);
				_retrieveIds(in ids, leaf, check);
				Pool.Release(leaf);
			}

			return new Cursor<ObjectId>(ids, CollectionLock);
		}
		
		private IEnumerable<ObjectId> _read(KeyCheckRange check, long take, bool asc, bool takeAll)
		{
			BTreeIndexKey search;
			if (asc)
			{
				search = ToBTreeIndexKey(check.LowerBound);
			}

			else
			{
				search = ToBTreeIndexKey(check.UpperBound);
			}

			if (!TryFind(search, out var traceback))
			{
				yield break;
			}

			var ids = new List<ObjectId>();
			var start = traceback.Current;
			foreach (
				var leaf in asc
				? ChainHelpers.EnumerateForwardsPinned<BTreeLeafPage>(Pool, start, release: false)
				: ChainHelpers.EnumerateBackwardsPinned<BTreeLeafPage>(Pool, start, release: false)
			)
			{
				_retrieveIds(in ids, leaf, check);

				Pool.Release(leaf);
				if (ids.Count > 0)
				{
					foreach (var id in ids)
					{
						if (takeAll)
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
		
		private bool _tryGetOriginalKey(ObjectId id, out NormalisedValue key)
		{
			var idn = new ObjectIdNormalised(id);
			if (ClusteredIndex.TryRead(idn, out var handle))
			{
				if (ObjectReader.TryRead(Pool, handle, id, ValueSelector.SelectAll, out var buffer))
				{
					return buffer.TryGetNormalisedValue(IndexedField.StringBufferValue, out key);
				}
			}

			key = default!;
			return false;
		}

		private void _retrieveIds(in List<ObjectId> ids, BTreeLeafPage from, KeyCheckRange check)
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
						check.LowerBound.AsSpan().Bytes.Length > Info.KeyMaxLength ||
						check.UpperBound.AsSpan().Bytes.Length > Info.KeyMaxLength
					)
				)
				{
					if (_tryGetOriginalKey(id, out var full))
					{
						result = check.Check(full.AsSpan());
					}
				}

				else
				{
					result =
						check.LowerBound.AsSpan().Bytes.Length <= Info.KeyMaxLength &&
						check.UpperBound.AsSpan().Bytes.Length <= Info.KeyMaxLength &&
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
						ChainHelpers.EnumerateForwardsPinned<BTreeLeafPageOverflow>(Pool, start, release: true)
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
