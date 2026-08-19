using System;

namespace Barbados.Documents.Tests
{
	internal sealed class BarbadosKeyTest
	{
		public sealed class Constructor
		{
			[Test]
			[TestCase("str")]
			[TestCase("str1.str2")]
			[TestCase("Μπαρμπάντος")]
			[TestCase("str.Μπαρμπάντος")]
			[TestCase("Μπαρμπάντος.str")]
			public void GivenString_CreatedCorrectly(string str)
			{
				var key = new BarbadosKey(str);
				var skey = key.ToString();
				Assert.That(skey, Is.EqualTo(str));
			}

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
