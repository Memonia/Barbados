using System;
using System.Collections.Generic;

namespace Barbados.StorageEngine
{
	public interface ICursor<T> : IEnumerable<T>, IDisposable
	{
		BarbadosIdentifier OwnerName { get; }

		void Close();
	}
}
