using System;

namespace Barbados.Documents.Tests
{
	public sealed partial class BarbadosDocumentTest
	{
		public sealed class Builder
		{
			public sealed class Add
			{
				[Test]
				public void FieldEmpty_ThrowsException()
				{
					var field = string.Empty;
					var builder = new BarbadosDocument.Builder();
					var value = "value";

					Assert.Throws<ArgumentException>(
						() => builder.Add(field, value)
					);
				}

				[Test]
				public void FieldDuplicate_ThrowsException()
				{
					var field = "duplicate";
					var builder = new BarbadosDocument.Builder();
					var value = "value";

					builder.Add(field, value);

					Assert.Throws<ArgumentException>(
						() => builder.Add(field, value)
					);
				}

				[Test]
				public void DocumentEmpty_ThrowsException()
				{
					var field = "empty-document";
					var builder = new BarbadosDocument.Builder();
					var document = BarbadosDocument.Empty;

					Assert.Throws<ArgumentException>(
						() => builder.Add(field, document)
					);
				}

				[Test]
				public void DocumentArrayEmpty_ThrowsException()
				{
					var field = "empty-array";
					var builder = new BarbadosDocument.Builder();
					var documentArray = Array.Empty<BarbadosDocument>();

					Assert.Throws<ArgumentException>(
						() => builder.Add(field, documentArray)
					);
				}

				[Test]
				public void DocumentArrayWithEmptyDocument_ThrowsException()
				{
					var field = "empty-document-in-array";
					var builder = new BarbadosDocument.Builder();
					var documentArray = new BarbadosDocument[] { BarbadosDocument.Empty };

					Assert.Throws<ArgumentException>(
						() => builder.Add(field, documentArray)
					);
				}
			}
		}
	}
}
