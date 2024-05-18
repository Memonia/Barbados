using System;

namespace Barbados.StorageEngine.Paging
{
	internal readonly struct PageBuffer
	{
		private readonly byte[] _bytes;

		public PageBuffer()
		{
			_bytes = new byte[Constants.PageLength];
		}

		public readonly Span<byte> AsSpan() => _bytes;
	}
}
