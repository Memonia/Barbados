using System.Diagnostics;
using System.Reflection;

using Barbados.StorageEngine.Documents.Binary;
using Barbados.StorageEngine.Paging.Pages;

namespace Barbados.StorageEngine.Helpers
{
	internal static class DebugHelpers
	{
		[Conditional("DEBUG")]
		public static void AssertBTreePageHeaderAllowedLength(int headerLength, int payloadFixedLengthPart)
		{
			// Ensure that the leaf page can fit a certain amount of keys, so that the splits don't fail
			var minKeyCount = 4;
			Debug.Assert(
				(Constants.SlottedPagePayloadLength - headerLength - payloadFixedLengthPart * minKeyCount) /
				Constants.IndexKeyMaxLength >= minKeyCount
			);
		}

		[Conditional("DEBUG")]
		public static void AssertObjectPageHeaderAllowedLength(int headerLength, int payloadFixedLengthPart)
		{
			// Same as above, but for the object page
			var minKeyCount = 4;
			Debug.Assert(
				(Constants.SlottedPagePayloadLength - headerLength - payloadFixedLengthPart * minKeyCount) /
				Constants.ObjectPageMaxChunkLength >= minKeyCount
			);
		}

		[Conditional("DEBUG")]
		public static void AssertSlottedPageHeaderLength()
		{
			var t = typeof(SlottedPage.SlottedPageHeader);
			var f = t.GetField("_bits", BindingFlags.NonPublic | BindingFlags.Instance);
			if (f == null)
			{
				Debug.Fail($"Outdated {nameof(SlottedPage.SlottedPageHeader)}");
			}

			Debug.Assert(
				f.FieldType == typeof(ulong) && SlottedPage.SlottedPageHeader.BinaryLength == sizeof(ulong)
			);
		}

		[Conditional("DEBUG")]
		public static void AssertSlotDescriptorLength()
		{
			var t = typeof(SlottedPage.Descriptor);
			var f = t.GetField("_bits", BindingFlags.NonPublic | BindingFlags.Instance);
			if (f == null)
			{
				Debug.Fail($"Outdated {nameof(SlottedPage.Descriptor)}");
			}

			Debug.Assert(
				f.FieldType == typeof(ulong) && SlottedPage.Descriptor.BinaryLength == sizeof(ulong)
			);
		}

		[Conditional("DEBUG")]
		public static void AssertValueDescriptorLength()
		{
			var t = typeof(ObjectBuffer.ValueDescriptor);
			var f = t.GetField("_bits", BindingFlags.NonPublic | BindingFlags.Instance);
			if (f == null)
			{
				Debug.Fail($"Outdated {nameof(ObjectBuffer.ValueDescriptor)}");
			}

			Debug.Assert(
				f.FieldType == typeof(ulong) && ObjectBuffer.ValueDescriptor.BinaryLength == sizeof(ulong)
			);
		}
	}
}
