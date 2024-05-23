using System;
using System.Collections.Generic;
using System.Diagnostics;

using Barbados.StorageEngine.Documents;
using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Indexing.Search;
using Barbados.StorageEngine.Paging;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Indexing
{
	internal sealed partial class BTreeIndex : AbstractBTreeIndex<BTreeLeafPage>, IBTreeIndexLookup
	{
		public BarbadosIdentifier Name { get; }
		public BarbadosIdentifier Collection { get; }
		public BarbadosIdentifier IndexedField { get; }

		IBarbadosController IBTreeIndexLookup.Controller => Controller;

		public BTreeIndex(
			BarbadosIdentifier name,
			BarbadosIdentifier collection,
			BarbadosIdentifier indexedField,
			BarbadosController controller,
			BTreeIndexInfo info
		) : base(controller, info)
		{
			Name = name;
			Collection = collection;
			IndexedField = indexedField;
		}

		public void DeallocateNoLock()
		{
			base.Deallocate();
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

			IKeyCheck check;
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
					check = KeyCheckFactory.GetBetweenCheck(start, end, KeyCheckCondition.BetweenInclusive);
					ids = _read(start, check, true);

				}

				else
				{
					check = KeyCheckFactory.GetBetweenCheck(start, end, KeyCheckCondition.Between);
					ids = _read(start, check, true);
				}
			}

			else
			if (doExact || !(doLess || doGreater || doRange))
			{
				_throwInvalidCondition(doLess || doGreater || doRange);
				var key = _retrieveSearchKey(condition);
				check = KeyCheckFactory.GetCheck(key, KeyCheckCondition.Equal);
				ids = _read(key, check, true);
			}

			else
			if (doLess)
			{
				_throwInvalidCondition(doExact || doGreater || doRange);
				var key = _retrieveSearchKey(condition);
				if (doInclusive)
				{
					check = KeyCheckFactory.GetCheck(key, KeyCheckCondition.LessThanOrEqual);
					ids = _read(key, check, false);

				}

				else
				{
					check = KeyCheckFactory.GetCheck(key, KeyCheckCondition.LessThan);
					ids = _read(key, check, false);
				}
			}

			else
			if (doGreater)
			{
				_throwInvalidCondition(doExact || doLess || doRange);
				var key = _retrieveSearchKey(condition);
				if (doInclusive)
				{
					check = KeyCheckFactory.GetCheck(key, KeyCheckCondition.GreaterThanOrEqual);
					ids = _read(key, check, true);
				}

				else
				{
					check = KeyCheckFactory.GetCheck(key, KeyCheckCondition.GreaterThan);
					ids = _read(key, check, true);
				}
			}

			else
			{
				throw new ArgumentException("Unexpected condition", nameof(condition));
			}

			return new Cursor<ObjectId>(ids, Controller.GetLock(Name, LockMode.Read));
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
				var leaf = Controller.Pool.LoadPin<BTreeLeafPage>(traceback.Current);
				var check = KeyCheckFactory.GetCheck(search, KeyCheckCondition.Equal);
				_writeMatchingIds(in ids, leaf, search.AsSpan(), check);
				Controller.Pool.Release(leaf);
			}

			return new Cursor<ObjectId>(ids, Controller.GetLock(Name, LockMode.Read));
		}

		private bool _tryRetrieveFullNormalisedKey(ObjectId id, out NormalisedValue key)
		{
			// Read-only collections include system collections
			var collection = Controller.GetReadOnlyCollection(Collection);
			if (collection.TryRead(id, out var document))
			{
				return document.Buffer.TryGetNormalisedValue(IndexedField.StringBufferValue, out key);
			}

			key = default!;
			return false;
		}

		private void _writeMatchingIds(in List<ObjectId> ids, BTreeLeafPage from, NormalisedValueSpan search, IKeyCheck check)
		{
			bool _check(NormalisedValueSpan search, BTreeIndexKey storedKey, ObjectId storedId)
			{
				var result = false;
				if (search.Bytes.Length > Info.KeyMaxLength && storedKey.IsTrimmed)
				{
					if (_tryRetrieveFullNormalisedKey(storedId, out var full))
					{
						result = check.Check(full.AsSpan());
					}
				}

				else
				{
					result =
						search.Bytes.Length <= Info.KeyMaxLength && !storedKey.IsTrimmed &&
						check.Check(storedKey.Separator);
				}

				return result;
			}

			Debug.Assert(ids.Count == 0);

			var e = from.GetEnumerator();
			while (e.TryGetNext(out var storedKey))
			{
				if (from.TryReadObjectId(storedKey.Separator, out var storedId, out _))
				{
					if (_check(search, storedKey, storedId))
					{
						ids.Add(storedId);
					}
				}

				else
				if (from.TryReadOverflowHandle(storedKey.Separator, out var start))
				{
					foreach (var overflow in
						ChainHelpers.EnumerateForwardsPinned<BTreeLeafPageOverflow>(Controller.Pool, start, release: true)
					)
					{
						var oe = overflow.GetEnumerator();
						while (oe.TryGetNext(out storedId, out var isTrimmed))
						{
							if (_check(search, storedKey, storedId))
							{
								ids.Add(storedId);
							}
						}
					}
				}
			}
		}

		private IEnumerable<ObjectId> _read(NormalisedValue search, IKeyCheck check, bool forwards)
		{
			var ikey = ToBTreeIndexKey(search);
			if (!TryFind(ikey, out var traceback))
			{
				yield break;
			}

			var ids = new List<ObjectId>();
			var start = traceback.Current;
			foreach (var leaf in forwards
				? ChainHelpers.EnumerateForwardsPinned<BTreeLeafPage>(Controller.Pool, start, release: false)
				: ChainHelpers.EnumerateBackwardsPinned<BTreeLeafPage>(Controller.Pool, start, release: false)
			)
			{
				_writeMatchingIds(ids, leaf, search.AsSpan(), check);

				Controller.Pool.Release(leaf);
				if (ids.Count > 0)
				{
					foreach (var id in ids)
					{
						yield return id;
					}

					ids.Clear();
				}

				else
				{
					break;
				}
			}
		}
	}
}
