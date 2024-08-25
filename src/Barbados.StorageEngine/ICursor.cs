using System;
using System.Collections.Generic;

namespace Barbados.StorageEngine
{
	public interface ICursor<T> : IDisposable, IEnumerable<T>
	{
		ObjectId CollectionId { get; }

		void Close();
	}
}
