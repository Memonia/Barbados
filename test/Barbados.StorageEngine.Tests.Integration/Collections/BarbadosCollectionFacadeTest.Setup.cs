using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration.Collections
{
	public partial class BarbadosCollectionFacadeTest
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

		public partial class TryUpdate(
			BarbadosContextFixture<TryUpdate> fixture
		) : IClassFixture<BarbadosContextFixture<TryUpdate>>
		{
			private readonly BarbadosContextFixture<TryUpdate> _fixture = fixture;
		}
	}
}
