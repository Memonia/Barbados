using System;

namespace Barbados.StorageEngine.Tests
{
	public sealed class BarbadosIdentifierTest
	{
		public sealed class Constructor
		{
			[Fact]
			public void GivenEmptyString_Throws()
			{
				var str = "";
				Assert.Throws<ArgumentException>(() => new BarbadosIdentifier(str));
			}

			[Fact]
			public void GivenNestingSeparator_Throws()
			{
				var str = new string([CommonIdentifiers.NestingSeparator]);
				Assert.Throws<ArgumentException>(() => new BarbadosIdentifier(str));
			}

			[Fact]
			public void GivenStringWhichStartsWithNestingSeparator_Throws()
			{
				var str = $"{CommonIdentifiers.NestingSeparator}test";
				Assert.Throws<ArgumentException>(() => new BarbadosIdentifier(str));
			}

			[Fact]
			public void GivenStringWhichContainsConsecutiveNestingSeparators_Throws()
			{
				var str =
					"test" +
					CommonIdentifiers.NestingSeparator +
					CommonIdentifiers.NestingSeparator +
					"test";
				Assert.Throws<ArgumentException>(() => new BarbadosIdentifier(str));
			}
		}

		public sealed class IsDocument
		{
			[Fact]
			public void GivenStringWhichEndsWithNestingSeparator_ReturnsTrue()
			{
				var str = "test" + CommonIdentifiers.NestingSeparator;
				var identifier = new BarbadosIdentifier(str);

				Assert.True(identifier.IsDocument);
			}

			[Fact]
			public void GivenStringDoesNotEndWithNestingSeparator_ReturnsFalse()
			{
				var str = "test";
				var identifier = new BarbadosIdentifier(str);

				Assert.False(identifier.IsDocument);
			}
		}

		public sealed class IsReserved
		{
			[Fact]
			public void GivenStringWhichStartsWithReservedNamePrefix_ReturnsTrue()
			{
				var str = CommonIdentifiers.ReservedNamePrefix + "test";
				var identifier = new BarbadosIdentifier(str);

				Assert.True(identifier.IsReserved);
			}

			[Fact]
			public void GivenStringDoesNotStartWithReservedNamePrefix_ReturnsFalse()
			{
				var str = "test";
				var identifier = new BarbadosIdentifier(str);

				Assert.False(identifier.IsReserved);
			}
		}

		public sealed class GetDocumentName
		{
			[Fact]
			public void GivenStringWhichEndsWithNestingSeparator_ReturnsStringWithoutNestingSeparator()
			{
				var name = "test";
				var str = name + CommonIdentifiers.NestingSeparator;
				var identifier = new BarbadosIdentifier(str);

				Assert.Equal(name, identifier.GetDocumentName());
			}

			[Fact]
			public void GivenStringHasNestingLevelTwoAndEndsWithNestingSeparator_ReturnsStringWithoutNestingSeparator()
			{
				var name = "test" + CommonIdentifiers.NestingSeparator + "test";
				var str = name + CommonIdentifiers.NestingSeparator;
				var identifier = new BarbadosIdentifier(str);

				Assert.Equal(name, identifier.GetDocumentName());
			}

			[Fact]
			public void GivenStringDoesNotEndWithNestingSeparator_Throws()
			{
				var str = "test";
				var identifier = new BarbadosIdentifier(str);

				Assert.Throws<InvalidOperationException>(() => identifier.GetDocumentName());
			}
		}

		public sealed class GetDocumentIdentifier
		{
			[Fact]
			public void GivenStringDoesNotEndWithNestingSeparator_ReturnsSameStringWhichEndsWithNestingSeparator()
			{
				var str = "test";
				var result = str + CommonIdentifiers.NestingSeparator;
				var identifier = new BarbadosIdentifier(str);

				Assert.Equal(result, identifier.GetDocumentIdentifier());
			}

			[Fact]
			public void GivenStringHasNestingLevelTwoAndDoesNotEndWithNestingSeparator_ReturnsSameStringWhichEndsWithNestingSeparator()
			{
				var str = "test" + CommonIdentifiers.NestingSeparator + "test";
				var result = str + CommonIdentifiers.NestingSeparator;
				var identifier = new BarbadosIdentifier(str);

				Assert.Equal(result, identifier.GetDocumentIdentifier());
			}

			[Fact]
			public void GivenStringEndsWithNestingSeparator_Throws()
			{
				var str = "test" + CommonIdentifiers.NestingSeparator;
				var identifier = new BarbadosIdentifier(str);

				Assert.Throws<InvalidOperationException>(() => identifier.GetDocumentIdentifier());
			}
		}

		public sealed class GetSplitIndices
		{
			[Fact]
			public void GivenStringWithoutNestingSeparator_ReturnsIndexAfterEndOfString()
			{
				var str = "test";
				var identifier = new BarbadosIdentifier(str);
				var result = new[] { 4 };
				var i = 0;
				var e = identifier.GetSplitIndices();
				while (e.MoveNext())
				{
					Assert.Equal(result[i], e.Current);
				}
			}

			[Fact]
			public void GivenStringWithOneNestingSeparator_ReturnsIndexAfterNestingSeparatorAndEnd()
			{
				var str = "test" + CommonIdentifiers.NestingSeparator + "test";
				var identifier = new BarbadosIdentifier(str);
				var result = new[] { 4, 9 };

				var i = 0;
				var e = identifier.GetSplitIndices();
				while (e.MoveNext())
				{
					Assert.Equal(result[i], e.Current);
					i += 1;
				}

				Assert.Equal(result.Length, i);
			}

			[Fact]
			public void GivenStringWithTwoNestingSeparators_ReturnsIndicesAfterNestingSeparatorsAndEnd()
			{
				var str = "test" + CommonIdentifiers.NestingSeparator + "test" + CommonIdentifiers.NestingSeparator + "test";
				var identifier = new BarbadosIdentifier(str);
				var result = new[] { 4, 9, 14 };
				var i = 0;
				var e = identifier.GetSplitIndices();
				while (e.MoveNext())
				{
					Assert.Equal(result[i], e.Current);
					i += 1;
				}

				Assert.Equal(result.Length, i);
			}
		}
	}
}
