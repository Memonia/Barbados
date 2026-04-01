using System;

namespace Barbados.Documents.Tests
{
	internal sealed class BarbadosKeyTest
	{
		public sealed class Constructor
		{
			[Test]
			public void GivenEmptyString_Throws()
			{
				var str = "";
				Assert.Throws<ArgumentException>(() => new BarbadosKey(str));
			}

			[Test]
			public void GivenNestingSeparator_Throws()
			{
				Assert.Throws<ArgumentException>(() => new BarbadosKey(BarbadosKey.NestingSeparator));
			}

			[Test]
			public void GivenStringStartsWithNestingSeparator_Throws()
			{
				var str = $"{BarbadosKey.NestingSeparator}test";
				Assert.Throws<ArgumentException>(() => new BarbadosKey(str));
			}

			[Test]
			public void GivenStringEndsWithNestingSeparator_Throws()
			{
				var str = $"test{BarbadosKey.NestingSeparator}";
				Assert.Throws<ArgumentException>(() => new BarbadosKey(str));
			}

			[Test]
			public void GivenStringContainsConsecutiveNestingSeparators_Throws()
			{
				var str =
					"test" +
					BarbadosKey.NestingSeparator +
					BarbadosKey.NestingSeparator +
					"test";

				Assert.Throws<ArgumentException>(() => new BarbadosKey(str));
			}
		}

		public sealed class Properties
		{
			[Test]
			public void GivenString_ReturnCorrectSearchPrefixes()
			{
				var str = "test";
				var key = new BarbadosKey(str);

				using (Assert.EnterMultipleScope())
				{
					Assert.That(key.ValueSearchPrefix.AsBytes().EndsWith(BarbadosKey.NestingSeparatorAsPrefix.AsBytes()), Is.False);
					Assert.That(key.DocumentSearchPrefix.AsBytes().EndsWith(BarbadosKey.NestingSeparatorAsPrefix.AsBytes()), Is.True);
				}
			}
		}
	}
}
