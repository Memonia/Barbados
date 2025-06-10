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
				var str = new string([BarbadosKey.NestingSeparator]);
				Assert.Throws<ArgumentException>(() => new BarbadosKey(str));
			}

			[Test]
			public void GivenStringWhichStartsWithNestingSeparator_Throws()
			{
				var str = $"{BarbadosKey.NestingSeparator}test";
				Assert.Throws<ArgumentException>(() => new BarbadosKey(str));
			}

			[Test]
			public void GivenStringWhichContainsConsecutiveNestingSeparators_Throws()
			{
				var str =
					"test" +
					BarbadosKey.NestingSeparator +
					BarbadosKey.NestingSeparator +
					"test";

				Assert.Throws<ArgumentException>(() => new BarbadosKey(str));
			}
		}

		public sealed class IsDocument
		{
			[Test]
			public void GivenStringWhichEndsWithNestingSeparator_ReturnsTrue()
			{
				var str = "test" + BarbadosKey.NestingSeparator;
				var key = new BarbadosKey(str);

				Assert.That(key.IsDocument, Is.True);
			}

			[Test]
			public void GivenStringDoesNotEndWithNestingSeparator_ReturnsFalse()
			{
				var str = "test";
				var key = new BarbadosKey(str);

				Assert.That(key.IsDocument, Is.False);
			}
		}

		public sealed class GetValueKey
		{
			[Test]
			public void GivenStringWhichEndsWithNestingSeparator_ReturnsStringWithoutNestingSeparator()
			{
				var name = "test";
				var str = name + BarbadosKey.NestingSeparator;
				var key = new BarbadosKey(str);

				var res = key.GetValueKey().ToString();

				Assert.That(res, Is.EqualTo(name));
			}

			[Test]
			public void GivenStringHasNestingLevelTwoAndEndsWithNestingSeparator_ReturnsStringWithoutNestingSeparator()
			{
				var name = "test" + BarbadosKey.NestingSeparator + "test";
				var str = name + BarbadosKey.NestingSeparator;
				var key = new BarbadosKey(str);

				var res = key.GetValueKey().ToString();

				Assert.That(res, Is.EqualTo(name));
			}
		}

		public sealed class GetDocumenKey
		{
			[Test]
			public void GivenStringDoesNotEndWithNestingSeparator_ReturnsSameStringWhichEndsWithNestingSeparator()
			{
				var str = "test";
				var result = str + BarbadosKey.NestingSeparator;
				var key = new BarbadosKey(str);

				var res = key.GetDocumentKey().ToString();

				Assert.That(res, Is.EqualTo(result));
			}

			[Test]
			public void GivenStringHasNestingLevelTwoAndDoesNotEndWithNestingSeparator_ReturnsSameStringWhichEndsWithNestingSeparator()
			{
				var str = "test" + BarbadosKey.NestingSeparator + "test";
				var result = str + BarbadosKey.NestingSeparator;
				var key = new BarbadosKey(str);

				var res = key.GetDocumentKey().ToString();

				Assert.That(res, Is.EqualTo(result));
			}
		}
	}
}
