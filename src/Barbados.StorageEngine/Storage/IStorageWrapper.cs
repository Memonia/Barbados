using System;

namespace Barbados.StorageEngine.Storage
{
	internal interface IStorageWrapper : IDisposable
	{
		public long Length { get; }

		public int Read(long offset, Span<byte> destination);
		public int Write(long offset, ReadOnlySpan<byte> data);

		public void Persist();
		public void Truncate(long length);
	}
}
