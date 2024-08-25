using System;

namespace Barbados.StorageEngine.Tests
{
	public sealed class ValueSelectorTest
	{
		public sealed class Constructor
		{
			[Fact]
			public void GivenDuplicateIdentifiers_Throws()
			{
				Assert.Throws<ArgumentException>(() => new ValueSelector("test", "test"));
			}

			[Fact]
			public void GivenGroupIdentifierAndSameValueIdentifier_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => new ValueSelector("test" + CommonIdentifiers.NestingSeparator, "test")
				);
			}
		}
	}
}