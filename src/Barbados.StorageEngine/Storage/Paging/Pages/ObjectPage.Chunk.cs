using System;

using Barbados.StorageEngine.Helpers;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal partial class ObjectPage
	{
		private readonly ref struct Chunk(Span<byte> entry)
		{
			private readonly Span<byte> _entry = entry;

			public readonly Span<byte> Object => _getObject();

			public int ObjectLength
			{
				readonly get => HelpRead.AsInt32(_getTotalLength());
				set => HelpWrite.AsInt32(_getTotalLength(), value);
			}

			public PageHandle OverflowHandle
			{
				readonly get => HelpRead.AsPageHandle(_getOverflowHandle());
				set => HelpWrite.AsPageHandle(_getOverflowHandle(), value);
			}

			private readonly Span<byte> _getObject() => _entry[(sizeof(int) + Constants.PageHandleLength)..];
			private readonly Span<byte> _getTotalLength() => _entry[..sizeof(int)];
			private readonly Span<byte> _getOverflowHandle() => _entry.Slice(sizeof(int), Constants.PageHandleLength);
		}
	}
}
