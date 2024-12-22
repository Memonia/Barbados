using System;

namespace Barbados.Documents.Tests
{
	public sealed class BarbadosKeySelectorTest
	{
		public sealed class Constructor
		{
			[Test]
			public void DuplicateKeys_Throws()
			{
				Assert.Throws<ArgumentException>(() => new BarbadosKeySelector("test", "test"));
			}

			[Test]
			public void DocumenKeyAndSameValueKey_Throws()
			{
				Assert.Throws<ArgumentException>(
					() => new BarbadosKeySelector("test" + BarbadosKey.NestingSeparator, "test")
				);
			}
		}
	}
}
