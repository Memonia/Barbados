﻿using System;

namespace Barbados.StorageEngine.Paging.Pages
{
	internal partial class SlottedPage
	{
		protected readonly ref struct Slot
		{
			public readonly Span<byte> Key { get; }
			public readonly Span<byte> Data { get; }
			public readonly Span<byte> FreeSpace { get; }

			public Slot(Descriptor descriptor, Span<byte> payload)
			{
				var slot = payload.Slice(descriptor.Offset, descriptor.Length);
				Key = slot[..descriptor.KeyLength];
				Data = slot.Slice(descriptor.KeyLength, descriptor.DataLength);
				FreeSpace = slot[(descriptor.KeyLength + descriptor.DataLength)..];
			}
		}
	}
}
