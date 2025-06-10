using Barbados.CommonUtils.BitManipulation;

namespace Barbados.StorageEngine.BTree.Pages
{
	internal partial class BTreeLeafPage
	{
		private struct Flags
		{
			private const byte _isKeyTrimmedMask   = 0b1000_0000;
			private const byte _entryTypeValueMask = 0b0000_0011;

			private const int _entyTypeValueShift = 0;

			public static explicit operator Flags(byte flags) => new(flags);
			public static explicit operator byte(Flags flags) => flags._flags;

			private byte _flags;

			public Flags(byte flags)
			{
				_flags = flags;
			}

			public Flags(bool isKeyTrimmed, EntryType entryType)
			{
				_flags = 0;
				IsKeyTrimmed = isKeyTrimmed;
				EntryType = entryType;
			}

			public bool IsKeyTrimmed
			{
				readonly get => _flags.Get(_isKeyTrimmedMask);
				private set => _flags.Set(_isKeyTrimmedMask, value);
			}

			public EntryType EntryType
			{
				readonly get => (EntryType)_flags.GetBits(_entryTypeValueMask, _entyTypeValueShift);
				private set => _flags.SetBits((byte)value, _entryTypeValueMask, _entyTypeValueShift);
			}
		}
	}
}
