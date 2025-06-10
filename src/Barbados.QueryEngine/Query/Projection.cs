using System;
using System.Collections.Generic;

namespace Barbados.QueryEngine.Query
{
	public sealed class Projection
	{
		internal bool Inclusive => _inclusive ?? true;
		internal List<string> Keys { get; }

		public bool? _inclusive;

		internal Projection()
		{
			Keys = [];
		}

		public Projection Include(params string[] keys)
		{
			if (_inclusive.HasValue && _inclusive.Value)
			{
				throw new InvalidOperationException("Cannot include a key in an exclusive projection");
			}

			_inclusive = true;
			Keys.AddRange(keys);
			return this;
		}

		public Projection Exclude(params string[] keys)
		{
			if (_inclusive.HasValue && !_inclusive.Value)
			{
				throw new InvalidOperationException("Cannot include a key in an inclusive projection");
			}

			_inclusive = false;
			Keys.AddRange(keys);
			return this;
		}

		internal KeySelection GetCurrentSelection()
		{
			if (_inclusive is null)
			{
				return KeySelection.All;
			}

			return new KeySelection(Keys, Inclusive);
		}
	}
}
