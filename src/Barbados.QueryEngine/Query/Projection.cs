using System.Collections.Generic;

using Barbados.Documents;

namespace Barbados.QueryEngine.Query
{
	internal sealed class Projection : IProjection
	{
		private readonly List<BarbadosKey> _keys = [];

		public BarbadosKeySelector GetSelector()
		{
			return new(_keys);
		}

		public IProjection Include(string field)
		{
			_keys.Add(field);
			return this;
		}
	}
}
