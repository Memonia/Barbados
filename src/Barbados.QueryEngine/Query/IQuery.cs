using System.Collections.Generic;

using Barbados.StorageEngine.Documents;

namespace Barbados.QueryEngine.Query
{
	public interface IQuery
	{
		IQuery Filter(IFilter filter);
		IQuery Project(IProjection projection);
		IQuery Take(long limit);
		IQuery Ascending();
		IQuery Descending();

		IEnumerable<BarbadosDocument> Execute();

		string Format();
		string FormatTranslated();
	}
}
