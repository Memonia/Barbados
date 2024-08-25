using System;
using System.IO;

namespace Barbados.StorageEngine.Storage
{
	internal sealed class StorageWrapperFactory
	{
		private readonly bool _inMemory;

		public StorageWrapperFactory(bool inMemory)
		{
			_inMemory = inMemory;
		}

		public IStorageWrapper Create(string path)
		{
			return Create(path, false);
		}	
		
		public IStorageWrapper Create(string path, bool @readonly)
		{
			if (_inMemory)
			{
				throw new NotImplementedException();
			}
			
			var fa = @readonly ? FileAccess.Read : FileAccess.ReadWrite;
			var handle = File.OpenHandle(path, FileMode.OpenOrCreate, fa);
			return new DiskStorageWrapper(handle);
		}
	}
}
