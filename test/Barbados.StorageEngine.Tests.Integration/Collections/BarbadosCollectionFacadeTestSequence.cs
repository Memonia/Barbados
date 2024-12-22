using System.Collections.Generic;

using Barbados.Documents;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	public sealed class BarbadosCollectionFacadeTestSequence
	{
		public string Name { get; private set; }
		public IEnumerable<BarbadosDocument> Documents { get; private set; }

		public BarbadosCollectionFacadeTestSequence(string name, IEnumerable<BarbadosDocument> documents)
		{
			Name = name;
			Documents = documents;
		}

		public override string ToString() => Name;
	}
}
