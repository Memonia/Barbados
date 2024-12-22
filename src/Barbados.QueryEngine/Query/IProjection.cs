using Barbados.Documents;

namespace Barbados.QueryEngine.Query
{
	public interface IProjection
	{
		IProjection Include(string field);

		internal BarbadosKeySelector GetSelector();
	}
}
