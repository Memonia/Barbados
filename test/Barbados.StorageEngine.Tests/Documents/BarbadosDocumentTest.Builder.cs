using Barbados.StorageEngine.Documents;

namespace Barbados.StorageEngine.Tests.Documents
{
	public partial class BarbadosDocumentTest
	{
		public sealed class Builder
		{
			public sealed class Add
			{

				[Fact]
				public void DocumentEmpty_ThrowsException()
				{
					var field = "empty-document";
					var builder = new BarbadosDocument.Builder();
					var document = BarbadosDocument.Empty;

					Assert.Throws<ArgumentException>(
						() => builder.Add(field, document)
					);
				}

				[Fact]
				public void DocumentArrayEmpty_ThrowsException()
				{
					var field = "empty-array";
					var builder = new BarbadosDocument.Builder();
					var documentArray = Array.Empty<BarbadosDocument>();

					Assert.Throws<ArgumentException>(
						() => builder.Add(field, documentArray)
					);
				}

				[Fact]
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
