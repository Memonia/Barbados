using Barbados.StorageEngine.Tests.Integration.Utility;

namespace Barbados.StorageEngine.Tests.Integration
{
	public partial class TransactionTest
	{
		private readonly BarbadosContextFixture<TransactionTest> _fixture;

		public TransactionTest(BarbadosContextFixture<TransactionTest> fixture)
		{
			_fixture = fixture;
		}
	}
}
