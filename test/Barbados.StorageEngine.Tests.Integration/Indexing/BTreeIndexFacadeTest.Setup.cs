using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration.Indexing
{
	public partial class BTreeIndexFacadeTest
	{
		public partial class Insert(
			BarbadosContextFixture<Insert> fixture
		) : IClassFixture<BarbadosContextFixture<Insert>>
		{
			private readonly BarbadosContextFixture<Insert> _fixture = fixture;
		}

		public partial class TryRemove(
			BarbadosContextFixture<TryRemove> fixture
		) : IClassFixture<BarbadosContextFixture<TryRemove>>
		{
			private readonly BarbadosContextFixture<TryRemove> _fixture = fixture;
		}

		public partial class Find(
			BarbadosContextFixture<Find> fixture
		) : IClassFixture<BarbadosContextFixture<Find>>
		{
			private readonly BarbadosContextFixture<Find> _fixture = fixture;
		}
	}
}
