using System;
using System.Buffers.Binary;

namespace Barbados.StorageEngine
{
	internal readonly struct ObjectIdNormalised
	{
		public static ObjectId FromNormalised(ReadOnlySpan<byte> source)
		{
			var normalised = BinaryPrimitives.ReadUInt64BigEndian(source);
			return new ObjectId(_restore(normalised));
		}

		private static long _restore(ulong normalised)
		{
			return (long)(normalised ^ 0x8000_0000_0000_0000);
		}

		private readonly ulong _normalisedValue;

		public ObjectIdNormalised(ObjectId id)
		{
			_normalisedValue = (ulong)id.Value ^ 0x8000_0000_0000_0000;
		}

		public ObjectId GetObjectId()
		{
			return new ObjectId(_restore(_normalisedValue));
		}

		public void WriteTo(Span<byte> destination)
		{
			BinaryPrimitives.WriteUInt64BigEndian(destination, _normalisedValue);
		}

		public int CompareTo(ObjectIdNormalised other)
		{
			return _normalisedValue.CompareTo(other._normalisedValue);
		}
	}
}
