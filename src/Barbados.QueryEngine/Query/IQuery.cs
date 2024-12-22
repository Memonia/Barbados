using System.Collections.Generic;

using Barbados.Documents;

namespace Barbados.QueryEngine.Query
{
	public interface IQuery
	{
		IQuery Filter(IFilter filter);
		IQuery Project(IProjection projection);

		IEnumerable<BarbadosDocument> Execute();

		string Format();
		string FormatTranslated();
	}
}
