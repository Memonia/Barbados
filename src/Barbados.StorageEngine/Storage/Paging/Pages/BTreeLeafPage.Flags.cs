using Barbados.CommonUtils.BitManipulation;

namespace Barbados.StorageEngine.Storage.Paging.Pages
{
	internal partial class BTreeLeafPage
	{
		internal struct Flags(byte flags)
		{
			private const byte _isTrimmedMask = 0b0000_0001;
			private const byte _hasDuplicateMask = 0b0000_0010;

			public static implicit operator byte(Flags eflags) => eflags._flags;

			private byte _flags = flags;

			public bool IsTrimmed
			{
				readonly get => _flags.Get(_isTrimmedMask);
				set => _flags.Set(_isTrimmedMask, value);
			}

			public bool HasDuplicate
			{
				readonly get => _flags.Get(_hasDuplicateMask);
				set => _flags.Set(_hasDuplicateMask, value);
			}
		}
	}
}
