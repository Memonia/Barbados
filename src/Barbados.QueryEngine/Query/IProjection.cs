using Barbados.StorageEngine;

namespace Barbados.QueryEngine.Query
{
	public interface IProjection
	{
		IProjection Include(BarbadosIdentifier field);

		internal ValueSelector GetSelector();
	}
}
