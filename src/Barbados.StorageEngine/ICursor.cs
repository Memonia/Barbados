using System;
using System.Collections.Generic;

namespace Barbados.StorageEngine
{
	public interface ICursor<T> : IDisposable, IEnumerable<T>
	{
		void Close();
	}
}
