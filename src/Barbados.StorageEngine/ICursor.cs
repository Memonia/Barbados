using System.Collections.Generic;

namespace Barbados.StorageEngine
{
	public interface ICursor<T> : IEnumerable<T>
	{
		BarbadosIdentifier OwnerName { get; }

		void Close();
	}
}
