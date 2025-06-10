using System.Collections.Generic;

using Barbados.Documents;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	internal partial class BaseBarbadosCollectionFacadeTest
	{
		private static BarbadosDocument.Builder _createBuilder(int numKeys)
		{
			var builder = new BarbadosDocument.Builder();
			for (var i = 0; i < numKeys; ++i)
			{
				builder.Add($"k{i}", i);
			}

			return builder;
		}

		private static List<BarbadosDocument.Builder> _createBuilders(int count)
		{
			var builders = new List<BarbadosDocument.Builder>(count);
			for (var i = 0; i < count; ++i)
			{
				builders.Add(_createBuilder(i));
			}

			return builders;
		}

		private static List<BarbadosDocument.Builder> _createBuilders(int count, BarbadosKey indexField)
		{
			var builders = new List<BarbadosDocument.Builder>(count);
			for (var i = 0; i < count; ++i)
			{
				var builder = _createBuilder(i);
				builder.Add(indexField, i);
				builders.Add(builder);
			}

			return builders;
		}
	}
}
