using System;

using Barbados.StorageEngine.Storage;

namespace Barbados.StorageEngine.BTree.Pages
{
	internal partial class BTreeLeafPage
	{
		public readonly struct OverflowInfo
		{
			public const int BinaryLength = sizeof(long) * 2;

			public static OverflowInfo ReadFrom(ReadOnlySpan<byte> source)
			{
				var i = 0;
				var sequenceCount = HelpRead.AsInt64(source[i..]);
				i += sizeof(long);
				var nextSequenceNumber = HelpRead.AsInt64(source[i..]);
				return new(sequenceCount, nextSequenceNumber);
			}

			public long SequenceCount { get; }
			public long NextSequenceNumber { get; }

			public OverflowInfo(long sequenceCount, long nextSequenceNumber)
			{
				SequenceCount = sequenceCount;
				NextSequenceNumber = nextSequenceNumber;
			}

			public void WriteTo(Span<byte> destination)
			{
				var i = 0;
				HelpWrite.AsInt64(destination[i..], SequenceCount);
				i += sizeof(long);
				HelpWrite.AsInt64(destination[i..], NextSequenceNumber);
			}
		}
	}
}
