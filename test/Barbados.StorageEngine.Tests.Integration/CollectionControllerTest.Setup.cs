using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration
{
	public partial class CollectionControllerTest
	{
		public partial class EnsureCreated(
			BarbadosContextFixture<EnsureCreated> fixture
		) : IClassFixture<BarbadosContextFixture<EnsureCreated>>
		{
			private readonly BarbadosContextFixture<EnsureCreated> _fixture = fixture;
		}

		public partial class EnsureDeleted(
			BarbadosContextFixture<EnsureDeleted> fixture
		) : IClassFixture<BarbadosContextFixture<EnsureDeleted>>
		{
			private readonly BarbadosContextFixture<EnsureDeleted> _fixture = fixture;
		}

		public partial class Exists(
			BarbadosContextFixture<Exists> fixture
		) : IClassFixture<BarbadosContextFixture<Exists>>
		{
			private readonly BarbadosContextFixture<Exists> _fixture = fixture;
		}

		public partial class Get(
			BarbadosContextFixture<Get> fixture
		) : IClassFixture<BarbadosContextFixture<Get>>
		{
			private readonly BarbadosContextFixture<Get> _fixture = fixture;
		}

		public partial class Create(
			BarbadosContextFixture<Create> fixture
		) : IClassFixture<BarbadosContextFixture<Create>>
		{
			private readonly BarbadosContextFixture<Create> _fixture = fixture;
		}

		public partial class Rename(
			BarbadosContextFixture<Rename> fixture
		) : IClassFixture<BarbadosContextFixture<Rename>>
		{
			private readonly BarbadosContextFixture<Rename> _fixture = fixture;
		}

		public partial class Delete(
			BarbadosContextFixture<Delete> fixture
		) : IClassFixture<BarbadosContextFixture<Delete>>
		{
			private readonly BarbadosContextFixture<Delete> _fixture = fixture;
		}
	}
}
