using System;
using System.IO;

using Microsoft.Win32.SafeHandles;

namespace Barbados.StorageEngine.Storage
{
	internal sealed class DiskWrapper : IStorageWrapper
	{
		public long Length => RandomAccess.GetLength(_handle);

		private readonly SafeFileHandle _handle;

		public DiskWrapper(SafeFileHandle handle)
		{
			_handle = handle;
		}

		public int Read(long offset, Span<byte> destination)
		{
			return RandomAccess.Read(_handle, destination, offset);
		}

		public int Write(long offset, ReadOnlySpan<byte> data)
		{
			RandomAccess.Write(_handle, data, offset);
			return data.Length;
		}

		public void Persist()
		{
			RandomAccess.FlushToDisk(_handle);
		}

		public void Truncate(long length)
		{
			RandomAccess.SetLength(_handle, length);
		}

		public void Dispose()
		{
			_handle.Dispose();
		}
	}
}
