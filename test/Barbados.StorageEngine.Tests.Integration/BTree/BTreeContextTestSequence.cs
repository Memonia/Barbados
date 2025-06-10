using System.Collections.Generic;

using Barbados.StorageEngine.BTree;

namespace Barbados.StorageEngine.Tests.Integration.BTree
{
	internal sealed class BTreeContextTestSequence
	{
		public string Name { get; }
		public IReadOnlyList<KeyValuePair<BTreeNormalisedValue, byte[]>> KeyDataPairs { get; }

		public BTreeContextTestSequence(string name, IReadOnlyList<KeyValuePair<BTreeNormalisedValue, byte[]>> keyDataPairs)
		{
			Name = name;
			KeyDataPairs = keyDataPairs;
		}

		public override string ToString() => Name;
	}
}
