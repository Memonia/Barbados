using System.Collections.Generic;

using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Query
{
	internal sealed class Projection : IProjection
	{
		private readonly List<BarbadosIdentifier> _identifiers = [];

		public ValueSelector GetSelector()
		{
			return new(_identifiers);
		}

		public IProjection Include(BarbadosIdentifier field)
		{
			_identifiers.Add(field);
			return this;
		}
	}
}
