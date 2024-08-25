using Barbados.StorageEngine.Helpers;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal partial class ObjectPage
	{
		public struct Flags(byte flags)
		{
			private const byte _isChunkMask = 0b0000_0001;

			public static implicit operator byte(Flags flags) => flags._flags;

			private byte _flags = flags;

			public bool IsChunk
			{
				readonly get => _flags.Get(_isChunkMask);
				set => _flags.Set(_isChunkMask, value);
			}
		}
	}
}
