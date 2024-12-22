namespace Barbados.StorageEngine.Tests.Integration.Utility
{
	[Parallelizable]
	public class SetupTeardownBarbadosContextTestClass<TTestClass> where TTestClass : SetupTeardownBarbadosContextTestClass<TTestClass>
	{
		protected BarbadosContext Context => _scbc.Context;

		private SelfCleanupBarbadosContext<TTestClass> _scbc;

		[SetUp]
		public void Setup()
		{
			_scbc = new SelfCleanupBarbadosContext<TTestClass>();
		}

		[TearDown]
		public void Teardown()
		{
			_scbc.Dispose();
		}
	}
}
