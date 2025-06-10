using System;
using System.Collections.Generic;

using Barbados.Documents;
using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Collections
{
	public sealed class FindOptionsBuilder
	{
		private bool _minInclusive;
		private bool _maxInclusive;
		private object? _minValue;
		private object? _maxValue;
		private long? _skip;
		private long? _limit;
		private bool? _reverse;
		private bool? _inclusiveProjection;
		private readonly List<BarbadosKey> _projection;

		public FindOptionsBuilder()
		{
			_minInclusive = false;
			_maxInclusive = false;
			_projection = [];
		}

		public FindOptions Build()
		{
			var min = _minValue is null ? null : BTreeNormalisedValue.Create(_minValue, isKeyExternal: false);
			var max = _maxValue is null ? null : BTreeNormalisedValue.Create(_maxValue, isKeyExternal: false);
			var btreeOpts = new BTreeFindOptions(min, max, _minInclusive, _maxInclusive)
			{
				Skip = _skip,
				Limit = _limit,
				Reverse = _reverse ?? false,
			};

			return new(btreeOpts, _projection ?? [], _inclusiveProjection, _minValue, _maxValue);
		}

		public FindOptionsBuilder Include(BarbadosKey key)
		{
			return _projectionAdd(inclusiveProjection: true, key);
		}

		public FindOptionsBuilder Include(params BarbadosKey[] keys)
		{
			return _projectionAdd(inclusiveProjection: true, keys);
		}

		public FindOptionsBuilder Exclude(BarbadosKey key)
		{
			return _projectionAdd(inclusiveProjection: false, key);
		}

		public FindOptionsBuilder Exclude(params BarbadosKey[] keys)
		{
			return _projectionAdd(inclusiveProjection: false, keys);
		}

		public FindOptionsBuilder Lt<T>(T value)
		{
			_resetMinMax();
			return Max(value, inclusive: false);
		}

		public FindOptionsBuilder Gt<T>(T value)
		{
			_resetMinMax();
			return Min(value, inclusive: false);
		}

		public FindOptionsBuilder Eq<T>(T value)
		{
			_resetMinMax();
			return Min(value, inclusive: true).Max(value, inclusive: true);
		}

		public FindOptionsBuilder LtEq<T>(T value)
		{
			_resetMinMax();
			return Max(value, inclusive: true);
		}

		public FindOptionsBuilder GtEq<T>(T value)
		{
			_resetMinMax();
			return Min(value, inclusive: true);
		}

		public FindOptionsBuilder Min<T>(T value, bool inclusive)
		{
			_minValue = value;
			_minInclusive = inclusive;
			return this;
		}

		public FindOptionsBuilder Max<T>(T value, bool inclusive)
		{
			_maxValue = value;
			_maxInclusive = inclusive;
			return this;
		}

		public FindOptionsBuilder Skip(long amount)
		{
			_skip = amount;
			return this;
		}

		public FindOptionsBuilder Limit(long amount)
		{
			_limit = amount;
			return this;
		}

		public FindOptionsBuilder Reverse()
		{
			_reverse = true;
			return this;
		}

		private void _resetMinMax()
		{
			_minValue = null;
			_maxValue = null;
			_minInclusive = false;
			_maxInclusive = false;
		}

		private FindOptionsBuilder _projectionAdd(bool inclusiveProjection, params BarbadosKey[] keys)
		{
			if (!_inclusiveProjection.HasValue)
			{
				_inclusiveProjection = inclusiveProjection;
			}

			if (_inclusiveProjection.Value != inclusiveProjection)
			{
				throw new InvalidOperationException("Cannot mix include and exclude projections");
			}

			_projection.AddRange(keys);
			return this;
		}
	}
}
