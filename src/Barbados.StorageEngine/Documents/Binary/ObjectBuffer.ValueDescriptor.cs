using Barbados.StorageEngine.Helpers;

namespace Barbados.StorageEngine.Documents.Binary
{
	internal partial class ObjectBuffer
	{
		public readonly struct ValueDescriptor
		{
			public const int BinaryLength = sizeof(ulong);

			static ValueDescriptor()
			{
				DebugHelpers.AssertValueDescriptorLength();
			}

			/* Each offset is assumed to be from the start of the respective table 
			 */

			private const ulong _valueOffsetMask = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111_1111_1111;
			private const ulong _nameOffsetMask  = 0b0000_0000_0000_0000_1111_1111_1111_1111_1111_1111_0000_0000_0000_0000_0000_0000;
			private const ulong _typeMarkerMask  = 0b0000_0000_1111_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
			private const ulong _isArrayMask     = 0b1000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;

			private const int _valueOffsetShift = 0;
			private const int _nameOffsetShift  = 24;
			private const int _markerShift      = 48;

			public readonly ulong Bits => _bits;

			private readonly ulong _bits;

			public ValueDescriptor(ulong bits)
			{
				_bits = bits;
			}

			public ValueDescriptor(int valueOffset, int nameOffset, ValueTypeMarker marker, bool isArray)
			{
				ValueOffset = valueOffset;
				NameOffset = nameOffset;
				Marker = marker;
				IsArray = isArray;
			}

			public int ValueOffset
			{
				readonly get => (int)_bits.GetBits(_valueOffsetMask, _valueOffsetShift);
				init => _bits.SetBits((uint)value, _valueOffsetMask, _valueOffsetShift);
			}

			public int NameOffset
			{
				readonly get => (int)_bits.GetBits(_nameOffsetMask, _nameOffsetShift);
				init => _bits.SetBits((uint)value, _nameOffsetMask, _nameOffsetShift);
			}

			public ValueTypeMarker Marker
			{
				readonly get => (ValueTypeMarker)_bits.GetBits(_typeMarkerMask, _markerShift);
				init => _bits.SetBits((byte)value, _typeMarkerMask, _markerShift);
			}

			public bool IsArray
			{
				readonly get => _bits.Get(_isArrayMask);
				init => _bits.Set(_isArrayMask, value);
			}
		}
	}
}
