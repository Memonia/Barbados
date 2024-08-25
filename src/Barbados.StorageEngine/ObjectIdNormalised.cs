using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Barbados.StorageEngine
{
	internal readonly struct ObjectIdNormalised
	{
		[InlineArray(sizeof(ulong))]
		private struct UInt64Buffer
		{
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Add readonly modifier
			private byte _f;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore IDE0051 // Remove unused private members
		}

		public static implicit operator ReadOnlySpan<byte>(in ObjectIdNormalised id) => id._value[..];

		public static ObjectId FromNormalised(ReadOnlySpan<byte> source)
		{
			return new ObjectId(
				(long)(BinaryPrimitives.ReadUInt64BigEndian(source) ^ 0x8000_0000_0000_0000)
			);
		}

		// Careful with defensive copies. 'UInt64Buffer' cannot be readonly (InlineArray)
		private readonly UInt64Buffer _value;

		public ObjectIdNormalised(ObjectId id)
		{
			_value = new();
			BinaryPrimitives.WriteUInt64BigEndian(_value, (ulong)id.Value ^ 0x8000_0000_0000_0000);
		}

		public int CompareTo(ObjectIdNormalised other)
		{
			return ((ReadOnlySpan<byte>)this).SequenceCompareTo(other);
		}
	}
}
