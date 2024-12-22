using System;

namespace Barbados.Documents.Tests
{
	public sealed class BarbadosDocumentGetTestCase
	{
		public string Name { get; private set; }
		public Func<bool> MustSucceed { get; private set; }
		public BarbadosDocument Document { get; private set; }

		internal BarbadosDocumentGetTestCase(string name, Func<bool> mustSucceed, BarbadosDocument doc)
		{
			Name = name;
			MustSucceed = mustSucceed;
			Document = doc;
		}

		public override string ToString() => Name;
	}
}
