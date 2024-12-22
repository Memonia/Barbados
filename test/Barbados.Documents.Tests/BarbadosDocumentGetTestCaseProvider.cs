using System.Collections;
using System.Collections.Generic;

namespace Barbados.Documents.Tests
{
	internal sealed class BarbadosDocumentGetTestCaseProvider : IEnumerable<BarbadosDocumentGetTestCase>
	{
		public IEnumerator<BarbadosDocumentGetTestCase> GetEnumerator()
		{
			throw new System.NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
