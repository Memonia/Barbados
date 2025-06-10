using System;

using Barbados.StorageEngine.Storage.Paging;

namespace Barbados.StorageEngine.Tests.Storage.Paging
{
	internal partial class SlottedPageTest
	{
		private sealed class SlottedPageFake : SlottedPage
		{

			public bool CanCompact => base.SlottedHeader.CanCompact;

			public SlottedPageFake() : this(0)
			{

			}

			public SlottedPageFake(ushort externalPayloadLength) : base(externalPayloadLength, new PageHeader(new(-1), PageMarker.Root))
			{

			}

			public new bool CanAllocate(int keyLength, int dataLength) => base.CanAllocate(keyLength, dataLength);
			public new bool TryRead(scoped ReadOnlySpan<byte> key, out Span<byte> data, out byte flags) => base.TryRead(key, out data, out flags);
			public new bool TryWrite(ReadOnlySpan<byte> key, ReadOnlySpan<byte> data) => base.TryWrite(key, data);
			public new bool TryRemove(ReadOnlySpan<byte> key) => base.TryRemove(key);
			public new bool TrySetFlags(ReadOnlySpan<byte> key, byte flags) => base.TrySetFlags(key, flags);
			public new bool TryAllocate(ReadOnlySpan<byte> key, int dataLength, out Span<byte> data) => base.TryAllocate(key, dataLength, out data);

			public override PageBuffer UpdateAndGetBuffer() => throw new NotImplementedException();
		}
	}
}
