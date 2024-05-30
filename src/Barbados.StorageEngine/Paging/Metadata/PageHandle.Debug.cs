using System.Diagnostics;

namespace Barbados.StorageEngine.Paging.Metadata
{
	internal readonly partial struct PageHandle
	{
		[Conditional("DEBUG")]
		public readonly void DEBUG_ThrowNullHandleDereference()
		{
			if (IsNull)
			{
				Debug.Fail("Null handle dereference");
			}
		}

		[Conditional("DEBUG")]
		public readonly void DEBUG_ThrowInvalidHandleDereference()
		{
			if (Handle < 0)
			{
				Debug.Fail($"Invalid handle dereference: {this}");
			}
		}

		public readonly void DEBUG_ThrowOutOfBoundsHandleDereference()
		{
			if (!IsWithinBounds)
			{
				Debug.Fail($"Outside bounds handle dereference: {this}");
			}
		}
	}
}
