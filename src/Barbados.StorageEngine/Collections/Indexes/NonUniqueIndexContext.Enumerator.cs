using Barbados.StorageEngine.BTree;
using Barbados.StorageEngine.Exceptions;
using Barbados.StorageEngine.Storage;

namespace Barbados.StorageEngine.Collections.Indexes
{
	internal partial class NonUniqueIndexContext
	{
		public new sealed class Enumerator : BaseIndexContext.Enumerator
		{
			private readonly BTreeFindOptions _options;
			private readonly BTreeContext.Enumerator _enum;

			private long _skip;
			private long _take;

			public Enumerator(NonUniqueIndexContext context, BTreeFindOptions options)
			{
				_options = options;

				// We are given search options for a non-unique index key, so we have
				// to create new bounds with the correct format for the underlying BTree
				var min = new KeyConcat(BTreeNormalisedValue.Min, options.Check.Min);
				var max = new KeyConcat(BTreeNormalisedValue.Max, options.Check.Max);
				var bopts = new BTreeFindOptions(
					new BTreeNormalisedValue(min.Bytes.ToArray()),
					new BTreeNormalisedValue(max.Bytes.ToArray()),
					_options.Check.IncludeMin,
					_options.Check.IncludeMax
				)
				{
					// See 'MoveNext'. We manually do 'take' and 'skip', because the
					// underlying BTree doesn't know about the non-unique index key format
					Skip = null,
					Limit = null,
					Reverse = _options.Reverse,
				};

				_enum = context.Context.GetDataEnumerator(bopts);
				_skip = _options.Skip ?? 0;
				_take = _options.Limit ?? long.MaxValue;
			}

			public override bool MoveNext()
			{
				if (_take <= 0)
				{
					return false;
				}

				while (_enum.MoveNext())
				{
					KeyConcat kc;
					if (!_enum.TryGetCurrentKeyAsSpan(out var span))
					{
						if (!_enum.TryGetCurrentKey(out var arr))
						{
							throw BarbadosInternalErrorExceptionHelpers.CouldNotRetrieveDataFromEnumeratorAfterMoveNext();
						}

						kc = new(arr.AsSpan().Bytes);
					}

					else
					{
						kc = new(span.Bytes);
					}

					var indexPortionLength = _getCurrentIndexPortionLength();
					var indexPortion = kc.GetIndexKeyPortion(indexPortionLength);

					// Some keys might match BTree's range check because of the primary key portion
					if (!_options.Check.Check(BTreeNormalisedValueSpan.FromNormalised(indexPortion)))
					{
						continue;
					}

					if (_skip > 0)
					{
						_skip -= 1;
						continue;
					}

					_take -= 1;
					return true;
				}

				return false;
			}

			public override bool TryGetCurrent(out BTreeNormalisedValue key)
			{
				if (!_enum.TryGetCurrentKey(out var k))
				{
					key = default!;
					return false;
				}

				var kc = new KeyConcat(k.AsSpan().Bytes);
				var iplen = _getCurrentIndexPortionLength();
				key = new BTreeNormalisedValue(kc.GetPrimaryKeyPortion(iplen).ToArray());
				return true;
			}

			public override bool TryGetCurrentAsSpan(out BTreeNormalisedValueSpan key)
			{
				if (!_enum.TryGetCurrentKeyAsSpan(out var k))
				{
					key = default;
					return false;
				}

				var kc = new KeyConcat(k.Bytes);
				var iplen = _getCurrentIndexPortionLength();
				key = BTreeNormalisedValueSpan.FromNormalised(kc.GetPrimaryKeyPortion(iplen));
				return true;
			}

			private int _getCurrentIndexPortionLength()
			{
				if (!_enum.TryGetCurrentDataAsSpan(out var lenSpan))
				{
					if (!_enum.TryGetCurrentData(out var lenArr))
					{
						throw BarbadosInternalErrorExceptionHelpers.CouldNotRetrieveDataFromEnumeratorAfterMoveNext();
					}

					lenSpan = lenArr;
				}

				return HelpRead.AsInt32(lenSpan);
			}
		}
	}
}
