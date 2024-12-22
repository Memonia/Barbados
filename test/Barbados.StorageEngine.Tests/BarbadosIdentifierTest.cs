namespace Barbados.StorageEngine.Tests
{
	public sealed class BarbadosDbObjectNameTest
	{
		public sealed class IsReserved
		{
			[Test]
			public void GivenStringWhichStartsWithReservedNamePrefix_ReturnsTrue()
			{
				var str = BarbadosDbObjects.ReservedNamePrefix + "test";
				var name = new BarbadosDbObjectName(str);

				Assert.That(name.IsReserved(), Is.True);
			}

			[Test]
			public void GivenStringDoesNotStartWithReservedNamePrefix_ReturnsFalse()
			{
				var str = "test";
				var name = new BarbadosDbObjectName(str);

				Assert.That(name.IsReserved(), Is.False);
			}
		}
	}
}
