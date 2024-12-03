using System.Diagnostics;

using Barbados.StorageEngine.Storage.Paging.Pages;

namespace Barbados.StorageEngine.Helpers
{
	internal static class DebugHelpers
	{
		[Conditional("DEBUG")]
		public static void AssertBTreePageMinKeyCount(int headerLength, int payloadFixedLengthPart)
		{
			// Ensure that the leaf page can fit a certain amount of keys, so that the splits don't fail
			var minKeyCount = 4;
			payloadFixedLengthPart += SlottedPage.Descriptor.BinaryLength;
			Debug.Assert(
				(Constants.SlottedPagePayloadLength - headerLength - payloadFixedLengthPart * minKeyCount) /
				Constants.IndexKeyMaxLength >= minKeyCount
			);
		}

		[Conditional("DEBUG")]
		public static void AssertObjectPageMinObjectCount(int headerLength, int payloadFixedLengthPart)
		{
			// Same as above, but for the object page
			var minKeyCount = 4;
			payloadFixedLengthPart += SlottedPage.Descriptor.BinaryLength;
			Debug.Assert(
				(Constants.SlottedPagePayloadLength - headerLength - payloadFixedLengthPart * minKeyCount) /
				Constants.ObjectPageMaxChunkLength >= minKeyCount
			);
		}
	}
}
