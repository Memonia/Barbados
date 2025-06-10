using System.Collections.Generic;
using System.Diagnostics;

using Barbados.Documents;
using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Collections
{
	public sealed class FindOptions
	{
		public static FindOptions All { get; } = new FindOptionsBuilder().Build();

		public static FindOptions Single<T>(T value)
		{
			return new FindOptionsBuilder().Eq(value).Build();
		}

		private static readonly BarbadosKey _minKey = "?min";
		private static readonly BarbadosKey _maxKey = "?max";
		private static readonly BarbadosKey _reverseKey = "?reverse";
		private static readonly BarbadosKey _skipKey = "?skip";
		private static readonly BarbadosKey _limitKey = "?limit";

		internal BTreeFindOptions Options { get; }
		internal bool? InclusiveProjection { get; }
		internal List<BarbadosKey> Projection { get; }

		private readonly object? _min;
		private readonly object? _max;

		internal FindOptions(
			BTreeFindOptions options,
			List<BarbadosKey> projection,
			bool? inclusiveProjection,
			object? min,
			object? max
		)
		{
			Debug.Assert(
				inclusiveProjection is null && projection.Count == 0 ||
				inclusiveProjection is not null && projection.Count > 0
			);

			Options = options;
			Projection = projection;
			InclusiveProjection = inclusiveProjection;
			_min = min;
			_max = max;
		}

		public BarbadosDocument AsDocument()
		{
			var builder = new BarbadosDocument.Builder();
			foreach (var key in Projection)
			{
				builder.Add(key, InclusiveProjection!);
			}

			builder.Add(_reverseKey, Options.Reverse);
			builder = Options.Skip is null ? builder : builder.Add(_skipKey, Options.Skip!);
			builder = Options.Limit is null ? builder : builder.Add(_limitKey, Options.Limit!);
			builder = _min is null ? builder : builder.Add(_minKey, _min!);
			builder = _max is null ? builder : builder.Add(_maxKey, _max!);
			return builder.Build();
		}
	}
}
